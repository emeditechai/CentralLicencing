using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AppUploadController : Controller
    {
        private static readonly string[] AllowedExtensions = { ".apk", ".ipa", ".aab" };
        private const long MaxFileSizeBytes = 500 * 1024 * 1024; // 500 MB

        private readonly IAppUploadRepository _repo;
        private readonly IWebHostEnvironment _env;

        public AppUploadController(IAppUploadRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var logs = (await _repo.GetAllAsync()).ToList();
            var androidLatest = await _repo.GetLatestByPlatformAsync("Android");
            var iosLatest = await _repo.GetLatestByPlatformAsync("iOS");

            ViewBag.AndroidLatest = androidLatest;
            ViewBag.IosLatest = iosLatest;
            return View(logs);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequestSizeLimit(524_288_000)]          // 500 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
        public async Task<IActionResult> Upload(string platform, IFormFile appFile, string? notes)
        {
            var logs = (await _repo.GetAllAsync()).ToList();
            var androidLatest = await _repo.GetLatestByPlatformAsync("Android");
            var iosLatest = await _repo.GetLatestByPlatformAsync("iOS");
            ViewBag.AndroidLatest = androidLatest;
            ViewBag.IosLatest = iosLatest;

            if (string.IsNullOrWhiteSpace(platform) ||
                (platform != "Android" && platform != "iOS"))
            {
                TempData["Error"] = "Please select a valid platform (Android or iOS).";
                return View("Index", logs);
            }

            if (appFile == null || appFile.Length == 0)
            {
                TempData["Error"] = "Please select a file to upload.";
                return View("Index", logs);
            }

            if (appFile.Length > MaxFileSizeBytes)
            {
                TempData["Error"] = $"File size exceeds the 500 MB limit.";
                return View("Index", logs);
            }

            var ext = Path.GetExtension(appFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                TempData["Error"] = "Only .apk, .aab, and .ipa files are allowed.";
                return View("Index", logs);
            }

            // Validate platform/extension match
            if (platform == "Android" && ext == ".ipa")
            {
                TempData["Error"] = "iOS (.ipa) files cannot be uploaded to the Android platform.";
                return View("Index", logs);
            }
            if (platform == "iOS" && (ext == ".apk" || ext == ".aab"))
            {
                TempData["Error"] = "Android (.apk / .aab) files cannot be uploaded to the iOS platform.";
                return View("Index", logs);
            }

            // Determine target folder
            var folderName = platform == "Android" ? "apk" : "ios";
            var targetDir = Path.Combine(_env.WebRootPath, "apps", folderName);
            Directory.CreateDirectory(targetDir);

            // Always replace: use a fixed canonical file name per platform
            var storedFileName = platform == "Android"
                ? $"app-release{ext}"
                : $"app-release{ext}";

            var targetPath = Path.Combine(targetDir, storedFileName);

            // Delete existing file before writing (replace)
            if (System.IO.File.Exists(targetPath))
                System.IO.File.Delete(targetPath);

            using (var stream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
            {
                await appFile.CopyToAsync(stream);
            }

            // Build the download URL (relative to wwwroot)
            var downloadUrl = $"/apps/{folderName}/{storedFileName}";

            var uploadedBy = User.FindFirstValue("FullName")
                ?? User.FindFirstValue(ClaimTypes.Name)
                ?? "Unknown";

            var log = new AppUploadLog
            {
                Platform = platform,
                FileName = storedFileName,
                OriginalName = Path.GetFileName(appFile.FileName),
                FileSizeBytes = appFile.Length,
                DownloadUrl = downloadUrl,
                UploadedBy = uploadedBy,
                Notes = notes?.Trim()
            };

            await _repo.AddAsync(log);

            TempData["Success"] = $"<strong>{platform}</strong> app uploaded successfully. Download link is now available.";
            return RedirectToAction(nameof(Index));
        }
    }
}

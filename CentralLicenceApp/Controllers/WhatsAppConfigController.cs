using System;
using System.Threading.Tasks;
using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class WhatsAppConfigController : Controller
    {
        private const int RecentLogCount = 25;

        private readonly IWhatsAppRepository _repo;
        private readonly IWhatsAppService _whatsApp;

        public WhatsAppConfigController(IWhatsAppRepository repo, IWhatsAppService whatsApp)
        {
            _repo = repo;
            _whatsApp = whatsApp;
        }

        // GET: /WhatsAppConfig
        public async Task<IActionResult> Index()
        {
            return View(new WhatsAppConfigViewModel
            {
                Config = await _repo.GetConfigAsync() ?? new WhatsAppConfiguration(),
                RecentLogs = await _repo.GetRecentLogsAsync(RecentLogCount)
            });
        }

        // POST: /WhatsAppConfig/Save
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(WhatsAppConfigViewModel model)
        {
            var existing = await _repo.GetConfigAsync();
            var newKey = model.NewApiKey?.Trim();
            if (string.IsNullOrEmpty(newKey) && existing?.HasApiKey != true)
                ModelState.AddModelError(nameof(model.NewApiKey), "API Token is required.");

            if (!ModelState.IsValid)
            {
                model.Config.Id = existing?.Id ?? 0;
                model.Config.ApiKeyEncrypted = existing?.ApiKeyEncrypted;
                model.Config.ApiKeyLast4 = existing?.ApiKeyLast4;
                model.Config.LastTestedDate = existing?.LastTestedDate;
                model.Config.LastTestResult = existing?.LastTestResult;
                model.RecentLogs = await _repo.GetRecentLogsAsync(RecentLogCount);
                return View(nameof(Index), model);
            }

            var cfg = model.Config;
            cfg.Id = existing?.Id ?? 0;
            cfg.ConfigName = cfg.ConfigName.Trim();
            cfg.ApiUrl = cfg.ApiUrl.Trim();
            cfg.AuthHeaderKey = string.IsNullOrWhiteSpace(cfg.AuthHeaderKey) ? "Authorization" : cfg.AuthHeaderKey.Trim();
            cfg.DefaultCountryCode = string.IsNullOrWhiteSpace(cfg.DefaultCountryCode) ? "+91" : cfg.DefaultCountryCode.Trim();
            cfg.SenderPhone = string.IsNullOrWhiteSpace(cfg.SenderPhone) ? null : cfg.SenderPhone.Trim();

            if (!string.IsNullOrEmpty(newKey))
            {
                if (newKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) newKey = newKey[7..].Trim();
                cfg.ApiKeyEncrypted = _whatsApp.ProtectApiKey(newKey);
                cfg.ApiKeyLast4 = newKey.Length >= 4 ? newKey[^4..] : newKey;
            }
            else
            {
                cfg.ApiKeyEncrypted = existing!.ApiKeyEncrypted;
                cfg.ApiKeyLast4 = existing.ApiKeyLast4;
            }

            await _repo.SaveConfigAsync(cfg, User.Identity?.Name ?? "system");
            TempData["Success"] = "WhatsApp configuration saved successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /WhatsAppConfig/SendTestMessage  (JSON, anti-forgery token in RequestVerificationToken header)
        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> SendTestMessage([FromBody] WhatsAppTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.ToPhone))
                return Json(new { success = false, message = "Please enter a recipient mobile number." });

            var cfg = await _repo.GetConfigAsync();
            var typedKey = request.ApiKey?.Trim();
            if (string.IsNullOrEmpty(typedKey) && cfg?.HasApiKey != true)
                return Json(new { success = false, message = "Enter the WaSender API token in the API Token box (or save the configuration first)." });

            var text = string.IsNullOrWhiteSpace(request.MessageText)
                ? $"Test message from Central Licence at {DateTime.Now:dd MMM yyyy HH:mm:ss}."
                : request.MessageText.Trim();

            var result = await _whatsApp.SendTextMessageAsync(request.ToPhone, text, "TEST",
                sentBy: User.Identity?.Name, ignoreMasterSwitch: true, overrideApiKey: typedKey);

            var summary = result.Success
                ? $"Success: {result.Status} ({result.MsgId})"
                : $"Failed: {result.ErrorMessage}";
            if (cfg != null) await _repo.UpdateTestResultAsync(cfg.Id, summary);

            return Json(new
            {
                logged = true,
                success = result.Success,
                message = result.Success ? $"Message accepted for {result.RecipientPhone}." : result.ErrorMessage,
                msgId = result.MsgId,
                jid = result.Jid,
                status = result.Status
            });
        }
    }
}

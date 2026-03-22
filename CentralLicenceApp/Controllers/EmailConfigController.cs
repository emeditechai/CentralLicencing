using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmailConfigController : Controller
    {
        private readonly IMailConfigRepository _repo;

        public EmailConfigController(IMailConfigRepository repo)
        {
            _repo = repo;
        }

        // GET: /EmailConfig
        public async Task<IActionResult> Index()
        {
            var configs = await _repo.GetAllAsync();
            return View(configs);
        }

        // GET: /EmailConfig/Create
        public IActionResult Create()
        {
            return View(new MailConfiguration { SmtpPort = 587, EnableSSL = true, IsActive = true });
        }

        // POST: /EmailConfig/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MailConfiguration model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = User.Identity?.Name ?? "system";
            await _repo.CreateAsync(model, user);
            TempData["Success"] = "Email configuration created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /EmailConfig/Edit/1
        public async Task<IActionResult> Edit(int id)
        {
            var config = await _repo.GetByIdAsync(id);
            if (config == null) return NotFound();
            return View(config);
        }

        // POST: /EmailConfig/Edit/1
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MailConfiguration model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var user = User.Identity?.Name ?? "system";
            await _repo.UpdateAsync(model, user);
            TempData["Success"] = "Email configuration updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /EmailConfig/ToggleActive
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, bool isActive)
        {
            await _repo.SetActiveAsync(id, isActive);
            TempData["Success"] = $"Configuration {(isActive ? "activated" : "deactivated")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

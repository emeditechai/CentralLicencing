using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmailConfigController : Controller
    {
        private readonly IMailConfigRepository _repo;
        private readonly IEmailService _emailService;

        public EmailConfigController(IMailConfigRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTest(int id, string testEmail)
        {
            var config = await _repo.GetByIdAsync(id);
            if (config == null) return NotFound();

            if (!config.IsActive)
            {
                TempData["Error"] = "Only the active email configuration can send a test email.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(testEmail))
            {
                TempData["Error"] = "Please provide a recipient email address for the test email.";
                return RedirectToAction(nameof(Index));
            }

            var subject = $"Email Engine Test - {config.FromName}";
            var body = $@"<div style='font-family:Segoe UI,Arial,sans-serif;font-size:14px;color:#1e293b;'>
<p>This is a test email sent from the Email Engine page.</p>
<p><strong>SMTP Server:</strong> {System.Net.WebUtility.HtmlEncode(config.SmtpServer)}</p>
<p><strong>From Name:</strong> {System.Net.WebUtility.HtmlEncode(config.FromName)}</p>
<p><strong>From Email:</strong> {System.Net.WebUtility.HtmlEncode(config.FromEmail)}</p>
<p><strong>Triggered By:</strong> {System.Net.WebUtility.HtmlEncode(User.Identity?.Name ?? "system")}</p>
<p><strong>Sent At:</strong> {System.DateTime.Now:dd MMM yyyy HH:mm:ss}</p>
</div>";

            await _emailService.SendAsync(testEmail, "Test Recipient", subject, body, "Email Engine Test");
            TempData["Success"] = $"Test email processed for <strong>{testEmail}</strong>. Check Email Logs for the audit entry.";
            return RedirectToAction(nameof(Index));
        }
    }
}

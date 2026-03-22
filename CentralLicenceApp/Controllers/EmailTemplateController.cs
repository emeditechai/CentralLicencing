using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmailTemplateController : Controller
    {
        private readonly IEmailTemplateRepository _repo;
        private readonly IEmailService _emailService;

        public EmailTemplateController(IEmailTemplateRepository repo, IEmailService emailService)
        {
            _repo         = repo;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var templates = await _repo.GetAllAsync();
            return View(templates);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var template = await _repo.GetByIdAsync(id);
            if (template == null) return NotFound();
            return View(template);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CentralLicenceApp.Models.EmailTemplate vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            await _repo.UpdateAsync(vm);
            TempData["Success"] = $"Template <strong>{vm.TemplateName}</strong> updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendTest(int id, string testEmail)
        {
            if (string.IsNullOrWhiteSpace(testEmail))
            {
                TempData["Error"] = "Please provide a test email address.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var template = await _repo.GetByIdAsync(id);
            if (template == null) return NotFound();

            var placeholders = new System.Collections.Generic.Dictionary<string, string>
            {
                ["ClientName"]    = "Test Client",
                ["ClientCode"]    = "CI-000001",
                ["ExpiryDate"]    = System.DateTime.Today.AddMonths(1).ToString("dd MMM yyyy"),
                ["AmcExpiryDate"] = System.DateTime.Today.AddMonths(1).ToString("dd MMM yyyy"),
                ["DaysRemaining"] = "7",
                ["AppName"]       = "eRestoPOS",
                ["AppUrl"]        = "http://your-app-url"
            };

            await _emailService.SendTemplatedAsync(template.TemplateKey, testEmail, "Test", placeholders);
            TempData["Success"] = $"Test email sent to <strong>{testEmail}</strong>.";
            return RedirectToAction(nameof(Edit), new { id });
        }
    }
}

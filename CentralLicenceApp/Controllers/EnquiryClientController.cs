using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    /// <summary>Enquiry Client Master: client base details, separate from Party Master.</summary>
    [Authorize(Roles = AccessRoles)]
    public class EnquiryClientController : EnquiryControllerBase
    {
        private readonly IEnquiryRepository _repo;

        public EnquiryClientController(IEnquiryRepository repo, IUserRepository userRepo, IPermissionService permissionService)
            : base(userRepo, permissionService) { _repo = repo; }

        public async Task<IActionResult> Index(string? search)
        {
            ViewBag.Search = search;
            return View(await _repo.GetClientsAsync(await GetScopeAsync(), search));
        }

        public async Task<IActionResult> Details(int id)
        {
            var scope = await GetScopeAsync();
            var client = await _repo.GetClientAsync(id, scope);
            if (client == null) return NotFound();
            ViewBag.Enquiries = (await _repo.GetEnquiriesAsync(scope, new EnquiryFilter())).Where(e => e.EnquiryClientId == id).ToList();
            return View(client);
        }

        [HttpGet]
        public async Task<IActionResult> Create(bool returnToEnquiry = false) => View("Form", new EnquiryClientFormViewModel
        {
            Client = new EnquiryClient { Contacts = new() { new EnquiryClientContact { IsPrimary = true } } },
            BusinessTypes = await _repo.GetLookupsAsync("EnquiryBusinessType", true),
            ReturnToEnquiry = returnToEnquiry
        });

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _repo.GetClientAsync(id, await GetScopeAsync());
            if (client == null) return NotFound();
            return View("Form", new EnquiryClientFormViewModel { Client = client, BusinessTypes = await _repo.GetLookupsAsync("EnquiryBusinessType", true) });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnquiryClientFormViewModel m) => await SaveAsync(m, 0);

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EnquiryClientFormViewModel m)
        {
            if (await _repo.GetClientAsync(id, await GetScopeAsync()) == null) return NotFound();
            return await SaveAsync(m, id);
        }

        private async Task<IActionResult> SaveAsync(EnquiryClientFormViewModel m, int id)
        {
            var c = m.Client;
            c.Id = id;
            c.CreatedByUserId = CurrentUserId;
            c.Contacts = (c.Contacts ?? new()).Where(k => !string.IsNullOrWhiteSpace(k.Name) || !string.IsNullOrWhiteSpace(k.Mobile)).ToList();

            string? error = null;
            if (string.IsNullOrWhiteSpace(c.CompanyName)) error = "Enter the company name.";
            else if (c.Contacts.Count == 0) error = "Add at least one contact.";
            else if (c.Contacts.Any(k => string.IsNullOrWhiteSpace(k.Name))) error = "Each contact needs a name.";
            else if (c.Contacts.All(k => string.IsNullOrWhiteSpace(k.Mobile) && string.IsNullOrWhiteSpace(k.Email))) error = "Add a mobile number or email for at least one contact.";

            if (error == null)
            {
                foreach (var k in c.Contacts)
                {
                    k.Mobile = k.Mobile?.Trim();
                    k.WhatsApp = k.WhatsApp?.Trim();
                    k.Email = k.Email?.Trim();
                }
                if (!c.Contacts.Any(k => k.IsPrimary)) c.Contacts[0].IsPrimary = true;
                var primary = c.Contacts.First(k => k.IsPrimary);
                foreach (var k in c.Contacts) k.IsPrimary = ReferenceEquals(k, primary);

                m.Duplicates = await _repo.FindDuplicatesAsync(id, c.GSTIN, c.Contacts.Select(k => k.Mobile ?? ""), c.Contacts.Select(k => k.Email ?? ""));
                if (m.Duplicates.Count > 0 && !m.ConfirmDuplicate)
                    error = "A client with the same details already exists. Review the matches, then tick the box to save anyway.";
            }

            if (error != null)
            {
                TempData["Error"] = error;
                if (c.Contacts.Count == 0) c.Contacts.Add(new EnquiryClientContact { IsPrimary = true });
                m.BusinessTypes = await _repo.GetLookupsAsync("EnquiryBusinessType", true);
                return View("Form", m);
            }

            c.CompanyName = c.CompanyName.Trim();
            var savedId = await _repo.SaveClientAsync(c);
            TempData["Success"] = "Client saved.";
            return m.ReturnToEnquiry
                ? RedirectToAction("Create", "ClientEnquiry", new { clientId = savedId })
                : RedirectToAction(nameof(Details), new { id = savedId });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> ToggleActive(int id, bool active)
        {
            if (await _repo.GetClientAsync(id, await GetScopeAsync()) == null) return NotFound();
            await _repo.SetClientActiveAsync(id, active);
            TempData["Success"] = active ? "Client activated." : "Client deactivated.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

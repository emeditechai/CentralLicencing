using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = EnquiryControllerBase.AccessRoles)]
    public class EnquiryMasterController : Controller
    {
        private readonly IEnquiryRepository _repo;

        public EnquiryMasterController(IEnquiryRepository repo) { _repo = repo; }

        public async Task<IActionResult> Index(string? tab)
        {
            ViewBag.Tab = tab ?? "stages";
            return View(new EnquiryMasterViewModel
            {
                Stages = await _repo.GetStagesAsync(false),
                Outcomes = await _repo.GetOutcomesAsync(false),
                Sources = await _repo.GetLookupsAsync("EnquirySource", false),
                BusinessTypes = await _repo.GetLookupsAsync("EnquiryBusinessType", false),
                LostReasons = await _repo.GetLookupsAsync("EnquiryLostReason", false)
            });
        }

        private static string? Validate(EnquiryLookup item) => string.IsNullOrWhiteSpace(item.Name) ? "Enter a name." : null;

        private async Task<IActionResult> SaveAsync(EnquiryLookup item, string tab, Func<Task> save)
        {
            var error = Validate(item);
            if (error != null) { TempData["Error"] = error; return RedirectToAction(nameof(Index), new { tab }); }
            item.Name = item.Name.Trim();
            try
            {
                await save();
                TempData["Success"] = $"{item.Name} saved.";
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2627 or 2601)
            {
                TempData["Error"] = $"{item.Name} already exists.";
            }
            return RedirectToAction(nameof(Index), new { tab });
        }

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public Task<IActionResult> SaveStage(EnquiryStage item) => SaveAsync(item, "stages", () => _repo.SaveStageAsync(item));

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public Task<IActionResult> SaveOutcome(EnquiryCallOutcome item) => SaveAsync(item, "outcomes", () => _repo.SaveOutcomeAsync(item));

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public Task<IActionResult> SaveSource(EnquiryLookup item) => SaveAsync(item, "sources", () => _repo.SaveLookupAsync("EnquirySource", item));

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public Task<IActionResult> SaveBusinessType(EnquiryLookup item) => SaveAsync(item, "types", () => _repo.SaveLookupAsync("EnquiryBusinessType", item));

        [HttpPost, ValidateAntiForgeryToken, RequiresPermission(Permissions.Edit)]
        public Task<IActionResult> SaveLostReason(EnquiryLookup item) => SaveAsync(item, "lost", () => _repo.SaveLookupAsync("EnquiryLostReason", item));
    }
}

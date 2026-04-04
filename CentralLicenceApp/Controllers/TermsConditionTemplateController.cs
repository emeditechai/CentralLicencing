using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class TermsConditionTemplateController : Controller
    {
        private readonly ITermsConditionTemplateRepository _repo;

        public TermsConditionTemplateController(ITermsConditionTemplateRepository repo)
        {
            _repo = repo;
        }

        // GET /TermsConditionTemplate
        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            return View(items.ToList());
        }

        // GET /TermsConditionTemplate/Create
        public IActionResult Create()
        {
            return View(new TermsConditionTemplateFormViewModel { IsActive = true });
        }

        // POST /TermsConditionTemplate/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TermsConditionTemplateFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (await _repo.IsNameExistsAsync(vm.TermsName))
            {
                ModelState.AddModelError(nameof(vm.TermsName), "A template with this Terms Name already exists.");
                return View(vm);
            }

            var template = new TermsConditionTemplate
            {
                TermsName   = vm.TermsName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive    = vm.IsActive
            };

            await _repo.CreateAsync(template);
            TempData["Success"] = $"Terms & Condition template <strong>{template.TermsName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        // GET /TermsConditionTemplate/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _repo.GetByIdAsync(id);
            if (template == null) return NotFound();

            return View(new TermsConditionTemplateFormViewModel
            {
                Id          = template.Id,
                TermsName   = template.TermsName,
                Description = template.Description,
                IsActive    = template.IsActive
            });
        }

        // POST /TermsConditionTemplate/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TermsConditionTemplateFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            if (await _repo.IsNameExistsAsync(vm.TermsName, excludeId: vm.Id))
            {
                ModelState.AddModelError(nameof(vm.TermsName), "A template with this Terms Name already exists.");
                return View(vm);
            }

            var template = new TermsConditionTemplate
            {
                Id          = vm.Id,
                TermsName   = vm.TermsName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive    = vm.IsActive
            };

            await _repo.UpdateAsync(template);
            TempData["Success"] = "Terms & Condition template updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST /TermsConditionTemplate/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted)
                TempData["Error"] = "The selected template was not found or could not be deleted.";
            else
                TempData["Success"] = "Terms & Condition template deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}

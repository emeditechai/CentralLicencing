using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin")]
    public class TicketSubCategoryController : Controller
    {
        private readonly ITicketSubCategoryRepository _repo;
        private readonly ITicketCategoryRepository _categoryRepo;

        public TicketSubCategoryController(ITicketSubCategoryRepository repo, ITicketCategoryRepository categoryRepo)
        {
            _repo = repo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            return View(items.ToList());
        }

        public async Task<IActionResult> Create()
        {
            var vm = new TicketSubCategoryFormViewModel
            {
                Categories = (await _categoryRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketSubCategoryFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = (await _categoryRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var subCategory = new TicketSubCategoryMaster
            {
                CategoryId = vm.CategoryId,
                SubCategoryName = vm.SubCategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(subCategory);
            TempData["Success"] = $"Ticket sub-category <strong>{subCategory.SubCategoryName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            var vm = new TicketSubCategoryFormViewModel
            {
                Id = item.Id,
                CategoryId = item.CategoryId,
                SubCategoryName = item.SubCategoryName,
                Description = item.Description,
                IsActive = item.IsActive,
                Categories = (await _categoryRepo.GetAllActiveAsync()).ToList()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketSubCategoryFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                vm.Categories = (await _categoryRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var subCategory = new TicketSubCategoryMaster
            {
                Id = vm.Id,
                CategoryId = vm.CategoryId,
                SubCategoryName = vm.SubCategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(subCategory);
            TempData["Success"] = "Ticket sub-category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (canDelete, reason) = await _repo.ValidateDeleteAsync(id);
            if (!canDelete)
            {
                TempData["Error"] = reason;
                return RedirectToAction(nameof(Index));
            }

            await _repo.DeleteAsync(id);
            TempData["Success"] = "Ticket sub-category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

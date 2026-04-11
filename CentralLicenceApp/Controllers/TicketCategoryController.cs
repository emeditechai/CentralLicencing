using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin")]
    public class TicketCategoryController : Controller
    {
        private readonly ITicketCategoryRepository _repo;

        public TicketCategoryController(ITicketCategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _repo.GetAllAsync();
            return View(categories.ToList());
        }

        public IActionResult Create()
        {
            return View(new TicketCategoryFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TicketCategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var category = new TicketCategoryMaster
            {
                CategoryName = vm.CategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(category);
            TempData["Success"] = $"Ticket category <strong>{category.CategoryName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var vm = new TicketCategoryFormViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TicketCategoryFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var category = new TicketCategoryMaster
            {
                Id = vm.Id,
                CategoryName = vm.CategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(category);
            TempData["Success"] = "Ticket category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var validation = await _repo.ValidateDeleteAsync(id);
            if (!validation.CanDelete)
            {
                TempData["Error"] = validation.Reason;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "The selected ticket category was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This ticket category cannot be deleted because related ticket records still reference it.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Ticket category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

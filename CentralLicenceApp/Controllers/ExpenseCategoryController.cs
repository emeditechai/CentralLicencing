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
    public class ExpenseCategoryController : Controller
    {
        private readonly IExpenseCategoryRepository _repo;

        public ExpenseCategoryController(IExpenseCategoryRepository repo)
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
            return View(new ExpenseCategoryFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseCategoryFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var category = new ExpenseCategoryMaster
            {
                CategoryName = vm.CategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(category);
            TempData["Success"] = $"Expense category <strong>{category.CategoryName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _repo.GetByIdAsync(id);
            if (category == null) return NotFound();

            var vm = new ExpenseCategoryFormViewModel
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseCategoryFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var category = new ExpenseCategoryMaster
            {
                Id = vm.Id,
                CategoryName = vm.CategoryName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(category);
            TempData["Success"] = "Expense category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Expense category deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
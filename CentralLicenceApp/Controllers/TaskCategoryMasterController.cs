using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin")]
    public class TaskCategoryMasterController : Controller
    {
        private readonly ITaskCategoryMasterRepository _repo;

        public TaskCategoryMasterController(ITaskCategoryMasterRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.CreateAsync(new TaskCategoryMaster { Name = name.Trim() });
            TempData["Success"] = $"Category <strong>{name.Trim()}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction(nameof(Index));
            }

            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = name.Trim();
            await _repo.UpdateAsync(item);

            TempData["Success"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            await _repo.ToggleActiveAsync(id);
            TempData["Success"] = $"Category '{item.Name}' is now {(item.IsActive ? "inactive" : "active")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound();

            if (await _repo.IsUsedAsync(id))
            {
                TempData["Error"] = $"Cannot delete '{item.Name}' — it is referenced by existing tasks. Deactivate it instead.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.DeleteAsync(id);
            TempData["Success"] = $"Category '{item.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

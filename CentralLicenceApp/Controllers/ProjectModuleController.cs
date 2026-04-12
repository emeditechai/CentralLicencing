using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin")]
    public class ProjectModuleController : Controller
    {
        private readonly IProjectModuleRepository _repo;

        public ProjectModuleController(IProjectModuleRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _repo.GetAllAsync();
            return View(projects);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Project / Module name is required.";
                return RedirectToAction(nameof(Index));
            }

            var project = new ProjectModuleMaster
            {
                Name = name.Trim(),
                Description = description?.Trim(),
                IsActive = true
            };

            await _repo.CreateAsync(project);
            TempData["Success"] = $"Project <strong>{project.Name}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Project / Module name is required.";
                return RedirectToAction(nameof(Index));
            }

            var project = await _repo.GetByIdAsync(id);
            if (project == null) return NotFound();

            project.Name = name.Trim();
            project.Description = description?.Trim();
            await _repo.UpdateAsync(project);

            TempData["Success"] = "Project updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var project = await _repo.GetByIdAsync(id);
            if (project == null) return NotFound();

            await _repo.ToggleActiveAsync(id);
            TempData["Success"] = $"Project '{project.Name}' is now {(project.IsActive ? "inactive" : "active")}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _repo.GetByIdAsync(id);
            if (project == null) return NotFound();

            if (await _repo.IsUsedAsync(id))
            {
                TempData["Error"] = $"Cannot delete '{project.Name}' — it is referenced by existing tasks. Deactivate it instead.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.DeleteAsync(id);
            TempData["Success"] = $"Project '{project.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

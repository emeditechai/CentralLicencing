using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class EmployeeDesignationController : Controller
    {
        private readonly IEmployeeDesignationRepository _repo;

        public EmployeeDesignationController(IEmployeeDesignationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var designations = await _repo.GetAllAsync();
            return View(designations.ToList());
        }

        public IActionResult Create()
        {
            return View(new EmployeeDesignationFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeDesignationFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var designation = new EmployeeDesignationMaster
            {
                DesignationName = vm.DesignationName.Trim(),
                Description     = vm.Description?.Trim(),
                IsActive        = vm.IsActive
            };

            await _repo.CreateAsync(designation);
            TempData["Success"] = $"Designation <strong>{designation.DesignationName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var designation = await _repo.GetByIdAsync(id);
            if (designation == null) return NotFound();

            var vm = new EmployeeDesignationFormViewModel
            {
                Id              = designation.Id,
                DesignationName = designation.DesignationName,
                Description     = designation.Description,
                IsActive        = designation.IsActive
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeDesignationFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var designation = new EmployeeDesignationMaster
            {
                Id              = vm.Id,
                DesignationName = vm.DesignationName.Trim(),
                Description     = vm.Description?.Trim(),
                IsActive        = vm.IsActive
            };

            await _repo.UpdateAsync(designation);
            TempData["Success"] = "Designation updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Designation deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
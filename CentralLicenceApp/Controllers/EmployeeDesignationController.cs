using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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
            var deleteValidation = await _repo.ValidateDeleteAsync(id);
            if (!deleteValidation.CanDelete)
            {
                TempData["Error"] = deleteValidation.Reason;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "The selected designation was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This designation cannot be deleted because related records still reference it. Reassign those records first.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Designation deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
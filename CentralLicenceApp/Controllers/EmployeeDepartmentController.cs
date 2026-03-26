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
    public class EmployeeDepartmentController : Controller
    {
        private readonly IEmployeeDepartmentRepository _repo;

        public EmployeeDepartmentController(IEmployeeDepartmentRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _repo.GetAllAsync();
            return View(departments.ToList());
        }

        public IActionResult Create()
        {
            return View(new EmployeeDepartmentFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeDepartmentFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var department = new EmployeeDepartmentMaster
            {
                DepartmentName = vm.DepartmentName.Trim(),
                Description    = vm.Description?.Trim(),
                IsActive       = vm.IsActive
            };

            await _repo.CreateAsync(department);
            TempData["Success"] = $"Department <strong>{department.DepartmentName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _repo.GetByIdAsync(id);
            if (department == null) return NotFound();

            var vm = new EmployeeDepartmentFormViewModel
            {
                Id             = department.Id,
                DepartmentName = department.DepartmentName,
                Description    = department.Description,
                IsActive       = department.IsActive
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeDepartmentFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var department = new EmployeeDepartmentMaster
            {
                Id             = vm.Id,
                DepartmentName = vm.DepartmentName.Trim(),
                Description    = vm.Description?.Trim(),
                IsActive       = vm.IsActive
            };

            await _repo.UpdateAsync(department);
            TempData["Success"] = "Department updated.";
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
                    TempData["Error"] = "The selected department was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This department cannot be deleted because related records still reference it. Reassign those records first.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Department deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
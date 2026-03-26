using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Department deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
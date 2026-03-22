using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleController : Controller
    {
        private readonly IRoleRepository _repo;

        public RoleController(IRoleRepository repo)
        {
            _repo = repo;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _repo.GetAllAsync();
            return View(roles.ToList());
        }

        public IActionResult Create()
        {
            return View(new RoleFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var role = new RoleMaster
            {
                RoleName    = vm.RoleName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive    = vm.IsActive
            };

            await _repo.CreateAsync(role);
            TempData["Success"] = $"Role <strong>{role.RoleName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _repo.GetByIdAsync(id);
            if (role == null) return NotFound();

            var vm = new RoleFormViewModel
            {
                Id          = role.Id,
                RoleName    = role.RoleName,
                Description = role.Description,
                IsActive    = role.IsActive
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RoleFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var role = new RoleMaster
            {
                Id          = vm.Id,
                RoleName    = vm.RoleName.Trim(),
                Description = vm.Description?.Trim(),
                IsActive    = vm.IsActive
            };

            await _repo.UpdateAsync(role);
            TempData["Success"] = "Role updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Role deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

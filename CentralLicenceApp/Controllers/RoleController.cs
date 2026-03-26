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
                    TempData["Error"] = "The selected role was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This role cannot be deleted because related records still reference it. Remove those links first.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Role deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

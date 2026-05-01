using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Filters;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class MenuManagementController : Controller
    {
        private readonly IMenuRepository _menuRepo;
        private readonly IPermissionMasterRepository _permRepo;
        private readonly IPermissionService _permService;

        public MenuManagementController(IMenuRepository menuRepo, IPermissionMasterRepository permRepo, IPermissionService permService)
        {
            _menuRepo = menuRepo; _permRepo = permRepo; _permService = permService;
        }

        public async Task<IActionResult> Index()
        {
            var menus = (await _menuRepo.GetAllAsync()).ToList();
            ViewBag.Permissions = (await _permRepo.GetAllAsync()).ToList();
            return View(menus);
        }

        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var perms = (await _permRepo.GetAllAsync()).ToList();
            var menus = (await _menuRepo.GetAllAsync()).ToList();
            var vm = new MenuUpsertViewModel { IsActive = true };
            if (id.HasValue)
            {
                var existing = await _menuRepo.GetByIdAsync(id.Value);
                if (existing == null) return NotFound();
                vm = new MenuUpsertViewModel
                {
                    Id = existing.Id, ParentId = existing.ParentId, MenuName = existing.MenuName,
                    MenuType = existing.MenuType, ControllerName = existing.ControllerName, ActionName = existing.ActionName,
                    IconClass = existing.IconClass, SortOrder = existing.SortOrder, IsActive = existing.IsActive,
                    PermissionIds = (await _menuRepo.GetPermissionIdsForMenuAsync(existing.Id)).ToList()
                };
            }
            ViewBag.Permissions = perms;
            ViewBag.ParentOptions = menus.Where(m => m.Id != vm.Id).ToList();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Upsert(MenuUpsertViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var perms = (await _permRepo.GetAllAsync()).ToList();
                var menus = (await _menuRepo.GetAllAsync()).ToList();
                ViewBag.Permissions = perms;
                ViewBag.ParentOptions = menus.Where(m => m.Id != vm.Id).ToList();
                return View(vm);
            }
            var entity = new MenuMaster
            {
                Id = vm.Id, ParentId = vm.ParentId, MenuName = vm.MenuName, MenuType = vm.MenuType,
                ControllerName = string.IsNullOrWhiteSpace(vm.ControllerName) ? null : vm.ControllerName,
                ActionName = string.IsNullOrWhiteSpace(vm.ActionName) ? null : vm.ActionName,
                IconClass = vm.IconClass, SortOrder = vm.SortOrder, IsActive = vm.IsActive
            };
            if (vm.Id == 0)
                entity.Id = await _menuRepo.CreateAsync(entity);
            else
                await _menuRepo.UpdateAsync(entity);

            await _menuRepo.SetMenuPermissionsAsync(entity.Id, vm.PermissionIds ?? new List<int>());
            _permService.InvalidateAll();
            TempData["SuccessMessage"] = vm.Id == 0 ? "Menu created." : "Menu updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequiresPermission(Permissions.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var children = await _menuRepo.CountChildrenAsync(id);
            if (children > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete: this menu has child items. Delete or move them first.";
                return RedirectToAction(nameof(Index));
            }
            await _menuRepo.DeleteAsync(id);
            _permService.InvalidateAll();
            TempData["SuccessMessage"] = "Menu deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

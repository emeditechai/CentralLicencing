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
    public class RolePermissionController : Controller
    {
        private readonly IRoleRepository _roleRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IPermissionMasterRepository _permRepo;
        private readonly IRolePermissionRepository _rolePermRepo;
        private readonly IPermissionService _permService;

        public RolePermissionController(IRoleRepository roleRepo, IMenuRepository menuRepo,
            IPermissionMasterRepository permRepo, IRolePermissionRepository rolePermRepo,
            IPermissionService permService)
        {
            _roleRepo = roleRepo; _menuRepo = menuRepo; _permRepo = permRepo;
            _rolePermRepo = rolePermRepo; _permService = permService;
        }

        public async Task<IActionResult> Index(int? roleId)
        {
            var roles = (await _roleRepo.GetAllAsync()).Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            var allPerms = (await _permRepo.GetAllAsync()).ToList();
            var allMenus = (await _menuRepo.GetAllAsync()).Where(m => m.IsActive).ToList();

            var selectedRoleId = roleId ?? roles.FirstOrDefault()?.Id ?? 0;
            var role = roles.FirstOrDefault(r => r.Id == selectedRoleId);

            var grants = selectedRoleId > 0
                ? (await _rolePermRepo.GetForRoleAsync(selectedRoleId))
                    .GroupBy(g => g.MenuId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionId).ToHashSet())
                : new Dictionary<int, HashSet<int>>();

            var menuPerms = await GetMenuPermissionMapAsync(allMenus.Select(m => m.Id));
            var rows = BuildRows(allMenus, menuPerms, allPerms);
            foreach (var r in rows)
                if (grants.TryGetValue(r.Menu.Id, out var set)) r.Granted = set;

            var vm = new RolePermissionGridViewModel
            {
                RoleId = selectedRoleId,
                RoleName = role?.RoleName ?? "",
                Roles = roles,
                Rows = rows,
                AllPermissions = allPerms,
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Save(int roleId, List<string> grants)
        {
            // grants entries are "menuId:permissionId"
            var pairs = (grants ?? new List<string>())
                .Select(s => s.Split(':', 2))
                .Where(a => a.Length == 2 && int.TryParse(a[0], out _) && int.TryParse(a[1], out _))
                .Select(a => (MenuId: int.Parse(a[0]), PermissionId: int.Parse(a[1])))
                .Distinct()
                .ToList();
            await _rolePermRepo.SaveForRoleAsync(roleId, pairs);
            _permService.InvalidateRole(roleId);
            TempData["SuccessMessage"] = "Role permissions saved.";
            return RedirectToAction(nameof(Index), new { roleId });
        }

        private async Task<Dictionary<int, HashSet<int>>> GetMenuPermissionMapAsync(IEnumerable<int> menuIds)
        {
            var dict = new Dictionary<int, HashSet<int>>();
            foreach (var id in menuIds)
            {
                var ids = (await _menuRepo.GetPermissionIdsForMenuAsync(id)).ToHashSet();
                dict[id] = ids;
            }
            return dict;
        }

        private static List<RolePermissionGridRow> BuildRows(List<MenuMaster> all, Dictionary<int, HashSet<int>> menuPerms, List<PermissionMaster> allPerms)
        {
            var byParent = all.ToLookup(m => m.ParentId);
            var rows = new List<RolePermissionGridRow>();
            void Walk(IEnumerable<MenuMaster> nodes, int depth)
            {
                foreach (var n in nodes.OrderBy(x => x.SortOrder))
                {
                    var permsForMenu = menuPerms.TryGetValue(n.Id, out var ids)
                        ? allPerms.Where(p => ids.Contains(p.Id)).ToList()
                        : new List<PermissionMaster>();
                    rows.Add(new RolePermissionGridRow { Menu = n, Depth = depth, Permissions = permsForMenu });
                    Walk(byParent[n.Id], depth + 1);
                }
            }
            Walk(byParent[null], 0);
            return rows;
        }
    }
}

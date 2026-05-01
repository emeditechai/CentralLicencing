using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class UserPermissionController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IMenuRepository _menuRepo;
        private readonly IPermissionMasterRepository _permRepo;
        private readonly IRolePermissionRepository _rolePermRepo;
        private readonly IUserPermissionRepository _userPermRepo;
        private readonly IPermissionService _permService;

        public UserPermissionController(IUserRepository userRepo, IRoleRepository roleRepo,
            IMenuRepository menuRepo, IPermissionMasterRepository permRepo,
            IRolePermissionRepository rolePermRepo, IUserPermissionRepository userPermRepo,
            IPermissionService permService)
        {
            _userRepo = userRepo; _roleRepo = roleRepo; _menuRepo = menuRepo;
            _permRepo = permRepo; _rolePermRepo = rolePermRepo; _userPermRepo = userPermRepo;
            _permService = permService;
        }

        public async Task<IActionResult> Index(int? userId, int? roleId)
        {
            var users = (await _userRepo.GetAllAsync()).Where(u => u.IsActive).OrderBy(u => u.Username).ToList();
            var roles = (await _roleRepo.GetAllAsync()).Where(r => r.IsActive).OrderBy(r => r.RoleName).ToList();
            var allPerms = (await _permRepo.GetAllAsync()).ToList();
            var allMenus = (await _menuRepo.GetAllAsync()).Where(m => m.IsActive).ToList();

            var selectedUserId = userId ?? users.FirstOrDefault()?.Id ?? 0;
            var selectedUser = users.FirstOrDefault(u => u.Id == selectedUserId);
            var assignedRoleIds = selectedUser?.AssignedRoleIds ?? new List<int>();
            // Default role context = primary role of the user
            var selectedRoleId = roleId ?? selectedUser?.RoleId ?? 0;
            var role = roles.FirstOrDefault(r => r.Id == selectedRoleId);

            // Fetch role grants for context role
            var roleGrants = selectedRoleId > 0
                ? (await _rolePermRepo.GetForRoleAsync(selectedRoleId))
                    .GroupBy(g => g.MenuId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionId).ToHashSet())
                : new Dictionary<int, HashSet<int>>();

            // Fetch user overrides
            var userOverrides = selectedUserId > 0
                ? (await _userPermRepo.GetForUserAsync(selectedUserId))
                    .GroupBy(o => o.MenuId)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.PermissionId, x => x.IsGranted))
                : new Dictionary<int, Dictionary<int, bool>>();

            var menuPerms = new Dictionary<int, HashSet<int>>();
            foreach (var m in allMenus)
                menuPerms[m.Id] = (await _menuRepo.GetPermissionIdsForMenuAsync(m.Id)).ToHashSet();

            var rows = BuildRows(allMenus, menuPerms, allPerms);
            foreach (var r in rows)
            {
                if (roleGrants.TryGetValue(r.Menu.Id, out var rg)) r.RoleGranted = rg;
                if (userOverrides.TryGetValue(r.Menu.Id, out var uo))
                {
                    foreach (var (pid, isGranted) in uo)
                        r.CellState[pid] = isGranted ? "Allow" : "Deny";
                }
            }

            var vm = new UserPermissionGridViewModel
            {
                UserId = selectedUserId,
                UserName = selectedUser?.Username ?? "",
                RoleId = selectedRoleId,
                RoleName = role?.RoleName ?? "",
                Users = users,
                Roles = assignedRoleIds.Any() ? roles.Where(r => assignedRoleIds.Contains(r.Id)).ToList() : roles,
                Rows = rows,
                AllPermissions = allPerms,
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [RequiresPermission(Permissions.Edit)]
        public async Task<IActionResult> Save(int userId, int roleId, List<string> overrides)
        {
            // overrides entries are "menuId:permissionId:Allow|Deny"
            var list = (overrides ?? new List<string>())
                .Select(s => s.Split(':', 3))
                .Where(a => a.Length == 3 && int.TryParse(a[0], out _) && int.TryParse(a[1], out _))
                .Select(a => new UserPermissionMap
                {
                    UserId = userId,
                    MenuId = int.Parse(a[0]),
                    PermissionId = int.Parse(a[1]),
                    IsGranted = string.Equals(a[2], "Allow", System.StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            var actor = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var aid) ? aid : (int?)null;
            await _userPermRepo.SaveForUserAsync(userId, actor, list);
            _permService.InvalidateUser(userId);
            TempData["SuccessMessage"] = "User overrides saved.";
            return RedirectToAction(nameof(Index), new { userId, roleId });
        }

        private static List<UserPermissionGridRow> BuildRows(List<MenuMaster> all, Dictionary<int, HashSet<int>> menuPerms, List<PermissionMaster> allPerms)
        {
            var byParent = all.ToLookup(m => m.ParentId);
            var rows = new List<UserPermissionGridRow>();
            void Walk(IEnumerable<MenuMaster> nodes, int depth)
            {
                foreach (var n in nodes.OrderBy(x => x.SortOrder))
                {
                    var permsForMenu = menuPerms.TryGetValue(n.Id, out var ids)
                        ? allPerms.Where(p => ids.Contains(p.Id)).ToList()
                        : new List<PermissionMaster>();
                    rows.Add(new UserPermissionGridRow { Menu = n, Depth = depth, Permissions = permsForMenu });
                    Walk(byParent[n.Id], depth + 1);
                }
            }
            Walk(byParent[null], 0);
            return rows;
        }
    }
}

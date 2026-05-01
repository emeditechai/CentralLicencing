using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.ViewComponents
{
    public class SideMenuViewComponent : ViewComponent
    {
        private readonly IPermissionService _perm;
        public SideMenuViewComponent(IPermissionService perm) { _perm = perm; }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true)
                return View(new SideMenuViewModel());

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleIdStr = user.FindFirstValue("ActiveRoleId");
            if (!int.TryParse(userIdStr, out var userId) || !int.TryParse(roleIdStr, out var roleId))
                return View(new SideMenuViewModel());

            // Super admin gets full menu tree (build effective grants from all menus)
            // Implementation: super admin will see ALL active menus regardless of grants.
            var tree = _perm.IsSuperAdmin(user)
                ? await BuildSuperAdminTreeAsync()
                : await _perm.GetMenuTreeForUserAsync(userId, roleId);

            var ctrl = (string?)RouteData.Values["controller"] ?? "";
            var act  = (string?)RouteData.Values["action"] ?? "";
            MarkActive(tree, ctrl, act);

            return View(new SideMenuViewModel { Roots = tree });
        }

        private async Task<System.Collections.Generic.List<Models.MenuMaster>> BuildSuperAdminTreeAsync()
        {
            // Use service config to read all active menus; reuse menu repo via DI.
            var repo = (Repositories.IMenuRepository)HttpContext.RequestServices
                .GetService(typeof(Repositories.IMenuRepository))!;
            var all = (await repo.GetAllAsync()).Where(m => m.IsActive).ToList();
            var byId = all.ToDictionary(m => m.Id);
            var roots = new System.Collections.Generic.List<Models.MenuMaster>();
            foreach (var m in all)
            {
                if (m.ParentId.HasValue && byId.TryGetValue(m.ParentId.Value, out var p))
                    p.Children.Add(m);
                else
                    roots.Add(m);
            }
            return roots;
        }

        private static bool MarkActive(System.Collections.Generic.List<Models.MenuMaster> nodes, string ctrl, string act)
        {
            var anyActive = false;
            foreach (var n in nodes)
            {
                var matches = !string.IsNullOrWhiteSpace(n.ControllerName)
                              && string.Equals(n.ControllerName, ctrl, System.StringComparison.OrdinalIgnoreCase)
                              && (string.IsNullOrWhiteSpace(n.ActionName)
                                  || string.Equals(n.ActionName, act, System.StringComparison.OrdinalIgnoreCase));
                var childActive = MarkActive(n.Children, ctrl, act);
                n.IsActiveItem = matches;
                n.IsExpanded = childActive || matches;
                anyActive |= n.IsActiveItem || childActive;
            }
            return anyActive;
        }
    }

    public class SideMenuViewModel
    {
        public System.Collections.Generic.List<Models.MenuMaster> Roots { get; set; } = new();
    }
}

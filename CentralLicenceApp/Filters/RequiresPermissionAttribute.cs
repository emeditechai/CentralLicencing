using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CentralLicenceApp.Filters
{
    /// <summary>
    /// Action-level permission requirement, e.g. [RequiresPermission("Create")].
    /// Resolves the menu by current controller/action and checks the supplied permission key
    /// against the user's effective permissions. Super-admin bypasses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public string PermissionKey { get; }
        public RequiresPermissionAttribute(string permissionKey) { PermissionKey = permissionKey; }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true) return;

            var permService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;
            if (permService == null) return;
            if (permService.IsSuperAdmin(user)) return;

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString();

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleIdStr = user.FindFirstValue("ActiveRoleId");
            if (!int.TryParse(userIdStr, out var userId) || !int.TryParse(roleIdStr, out var roleId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var ok = await permService.HasPermissionAsync(userId, roleId, controller, action, PermissionKey);
            if (!ok)
            {
                if (IsAjax(context.HttpContext.Request))
                    context.Result = new JsonResult(new { success = false, message = $"Permission '{PermissionKey}' denied." }) { StatusCode = 403 };
                else
                    context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }

        private static bool IsAjax(Microsoft.AspNetCore.Http.HttpRequest req)
            => string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || (req.Headers["Accept"].ToString()?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false);
    }
}

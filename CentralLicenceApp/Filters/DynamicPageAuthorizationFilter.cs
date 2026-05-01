using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CentralLicenceApp.Filters
{
    /// <summary>
    /// Global filter that enforces dynamic permissions on every MVC action.
    /// - Skips unauthenticated users (lets [Authorize] redirect).
    /// - Skips super admin and a small whitelist (Account, Home, Error, etc.).
    /// - Skips actions decorated with [AllowAnonymous] or [SkipDynamicAuthorization].
    /// - For actions decorated with [RequiresPermission(...)], that attribute runs and this is a no-op.
    /// - Otherwise, the required permission is inferred from action name + HTTP method
    ///   (Create / Edit / Delete / Approve / Reject / Cancel / Refund / Settle / Reimburse / Export / Print / View).
    /// AJAX requests get a 403 JSON body; full requests are redirected to /Account/AccessDenied.
    /// </summary>
    public class DynamicPageAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private static readonly string[] WhitelistControllers =
            { "Account", "Home", "Error", "PushNotification", "Help" };

        // Action-name prefixes mapped to permission keys.
        // Order matters: longer/more-specific prefixes first.
        private static readonly (string Prefix, string Perm)[] ActionPrefixMap =
        {
            ("Reimburse", Permissions.Reimburse),
            ("Settle",    Permissions.Settle),
            ("Refund",    Permissions.Refund),
            ("Cancel",    Permissions.Cancel),
            ("Approve",   Permissions.Approve),
            ("Reject",    Permissions.Reject),
            ("Export",    Permissions.Export),
            ("Download",  Permissions.Export),
            ("Print",     Permissions.Print),
            ("Delete",    Permissions.Delete),
            ("Remove",    Permissions.Delete),
            ("Destroy",   Permissions.Delete),
            ("Edit",      Permissions.Edit),
            ("Update",    Permissions.Edit),
            ("Modify",    Permissions.Edit),
            ("Toggle",    Permissions.Edit),
            ("Set",       Permissions.Edit),
            ("Save",      Permissions.Edit),     // Save without explicit Create/Edit context → treat as Edit
            ("Create",    Permissions.Create),
            ("Add",       Permissions.Create),
            ("New",       Permissions.Create),
            ("Insert",    Permissions.Create),
            ("Upload",    Permissions.Create),
            ("Import",    Permissions.Create),
        };

        private readonly IPermissionService _permissionService;

        public DynamicPageAuthorizationFilter(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // [AllowAnonymous] anywhere → skip
            if (context.Filters.OfType<IAllowAnonymousFilter>().Any()) return;

            // Explicit opt-out attribute
            if (context.ActionDescriptor.EndpointMetadata.OfType<SkipDynamicAuthorizationAttribute>().Any()) return;

            // [RequiresPermission] handles its own check; let it run instead.
            if (context.ActionDescriptor.EndpointMetadata.OfType<RequiresPermissionAttribute>().Any()) return;

            var user = context.HttpContext.User;
            if (user?.Identity?.IsAuthenticated != true) return; // [Authorize] handles redirect

            // Super-admin bypass
            if (_permissionService.IsSuperAdmin(user)) return;

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";

            if (WhitelistControllers.Contains(controller, StringComparer.OrdinalIgnoreCase)) return;

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleIdStr = user.FindFirstValue("ActiveRoleId");
            if (!int.TryParse(userIdStr, out var userId) || !int.TryParse(roleIdStr, out var roleId))
            {
                Deny(context);
                return;
            }

            // Determine the required permission key.
            var requiredPerm = InferPermission(action, context.HttpContext.Request.Method);

            var allowed = await _permissionService.HasPermissionAsync(userId, roleId, controller, action, requiredPerm);
            if (!allowed)
            {
                Deny(context);
            }
        }

        private static string InferPermission(string action, string httpMethod)
        {
            // POST/PUT/DELETE without a recognized prefix still defaults to View; but most write actions
            // do start with Create/Edit/Delete/Save in this codebase.
            foreach (var (prefix, perm) in ActionPrefixMap)
            {
                if (action.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return perm;
            }
            // Default: page-level read access.
            return Permissions.View;
        }

        private static void Deny(AuthorizationFilterContext context)
        {
            var req = context.HttpContext.Request;
            var isAjax = string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                         || (req.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase));
            if (isAjax)
            {
                context.Result = new JsonResult(new { success = false, message = "You do not have permission to perform this action." })
                {
                    StatusCode = 403
                };
            }
            else
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }

    /// <summary>
    /// Decorate an action or controller with this attribute to opt out of the global
    /// <see cref="DynamicPageAuthorizationFilter"/> permission check.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SkipDynamicAuthorizationAttribute : Attribute { }
}

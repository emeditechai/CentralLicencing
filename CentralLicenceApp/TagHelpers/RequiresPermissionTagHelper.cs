using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace CentralLicenceApp.TagHelpers
{
    /// <summary>
    /// Suppresses an element if the current user does not have the specified permission.
    /// Usage: &lt;a asp-permission="Invoice:Create"&gt;...&lt;/a&gt;
    /// Format: "Controller:PermissionKey" or "Controller/Action:PermissionKey".
    /// </summary>
    [HtmlTargetElement("*", Attributes = "asp-permission")]
    public class RequiresPermissionTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _http;
        private readonly IPermissionService _perm;

        public RequiresPermissionTagHelper(IHttpContextAccessor http, IPermissionService perm)
        {
            _http = http;
            _perm = perm;
        }

        [HtmlAttributeName("asp-permission")]
        public string? Permission { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.RemoveAll("asp-permission");
            if (string.IsNullOrWhiteSpace(Permission)) return;

            var user = _http.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) { output.SuppressOutput(); return; }
            if (_perm.IsSuperAdmin(user)) return;

            var parts = Permission.Split(':', 2);
            if (parts.Length != 2) return;
            var route = parts[0];
            var permKey = parts[1];
            string controller = route, action = "Index";
            var slash = route.IndexOf('/');
            if (slash > 0) { controller = route[..slash]; action = route[(slash + 1)..]; }

            var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var roleIdStr = user.FindFirstValue("ActiveRoleId");
            if (!int.TryParse(userIdStr, out var userId) || !int.TryParse(roleIdStr, out var roleId))
            { output.SuppressOutput(); return; }

            var ok = await _perm.HasPermissionAsync(userId, roleId, controller, action, permKey);
            if (!ok) output.SuppressOutput();
        }
    }
}

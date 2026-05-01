using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Services
{
    public interface IPermissionService
    {
        /// <summary>True if the principal is the super-admin (username "admin" or IsSuperAdmin claim).</summary>
        bool IsSuperAdmin(ClaimsPrincipal principal);

        Task<EffectivePermissionSet> GetEffectivePermissionsAsync(int userId, int roleId);

        /// <summary>Returns hierarchical menu tree the user/role can see (View granted).</summary>
        Task<List<MenuMaster>> GetMenuTreeForUserAsync(int userId, int roleId);

        Task<bool> HasPageAccessAsync(int userId, int roleId, string controller, string? action);
        Task<bool> HasPermissionAsync(int userId, int roleId, string controller, string? action, string permissionKey);

        void InvalidateUser(int userId);
        void InvalidateRole(int roleId);
        void InvalidateAll();
    }
}

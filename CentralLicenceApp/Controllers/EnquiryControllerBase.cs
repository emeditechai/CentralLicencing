using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    /// <summary>Shared by the Client Enquiry controllers: who the user is and what they may see.</summary>
    public abstract class EnquiryControllerBase : Controller
    {
        /// <summary>The only roles allowed into the Client Enquiry module.</summary>
        public const string AccessRoles = "Administrator,Ticket Admin";

        protected readonly IUserRepository UserRepo;
        protected readonly IPermissionService PermissionService;

        protected EnquiryControllerBase(IUserRepository userRepo, IPermissionService permissionService)
        {
            UserRepo = userRepo;
            PermissionService = permissionService;
        }

        protected int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

        /// <summary>Administrators and Ticket Admins see everything; everyone else sees their own and their reports' records.</summary>
        protected async Task<EnquiryScope> GetScopeAsync()
        {
            var userId = CurrentUserId;
            var all = PermissionService.IsSuperAdmin(User) || User.IsInRole("Administrator") || User.IsInRole("Ticket Admin");
            var ids = all ? new List<int> { userId } : (await UserRepo.GetSelfAndSubordinateIdsAsync(userId)).ToList();
            if (!ids.Contains(userId)) ids.Add(userId);
            return new EnquiryScope { All = all, UserId = userId, UserIds = ids };
        }

        protected async Task<List<UserMaster>> GetAssignableUsersAsync(EnquiryScope scope)
        {
            // Only people who can open the module can own, host or collaborate on an enquiry.
            var allowed = AccessRoles.Split(',');
            var users = (await UserRepo.GetAllAsync()).Where(u => u.IsActive && u.ClientId == null
                && (allowed.Contains(u.RoleName, StringComparer.OrdinalIgnoreCase)
                    || u.Roles.Any(r => allowed.Contains(r.RoleName, StringComparer.OrdinalIgnoreCase))));
            if (!scope.All) users = users.Where(u => scope.UserIds.Contains(u.Id));
            return users.OrderBy(u => u.FullName).ToList();
        }
    }
}

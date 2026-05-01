using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<RolePermissionMap>> GetForRoleAsync(int roleId);
        Task SaveForRoleAsync(int roleId, IEnumerable<(int MenuId, int PermissionId)> grants);
        Task GrantAsync(int roleId, int menuId, int permissionId);
    }
}

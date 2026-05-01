using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IUserPermissionRepository
    {
        Task<IEnumerable<UserPermissionMap>> GetForUserAsync(int userId);
        Task SaveForUserAsync(int userId, int? createdBy, IEnumerable<UserPermissionMap> overrides);
    }
}

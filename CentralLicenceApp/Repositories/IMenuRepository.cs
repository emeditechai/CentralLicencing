using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuMaster>> GetAllAsync();
        Task<MenuMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(MenuMaster menu);
        Task<bool> UpdateAsync(MenuMaster menu);
        Task<bool> DeleteAsync(int id);
        Task<int> CountChildrenAsync(int id);
        Task<IEnumerable<int>> GetPermissionIdsForMenuAsync(int menuId);
        Task SetMenuPermissionsAsync(int menuId, IEnumerable<int> permissionIds);
        Task<MenuMaster?> ResolveByRouteAsync(string controller, string? action);
    }
}

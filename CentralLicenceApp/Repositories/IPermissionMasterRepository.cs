using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IPermissionMasterRepository
    {
        Task<IEnumerable<PermissionMaster>> GetAllAsync();
        Task<PermissionMaster?> GetByKeyAsync(string key);
    }
}

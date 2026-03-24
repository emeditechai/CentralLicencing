using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ILocationRepository
    {
        Task<IEnumerable<LocationMaster>> GetAllActiveAsync();
    }
}

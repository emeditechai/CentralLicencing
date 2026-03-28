using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IPricingModelRepository
    {
        Task<IEnumerable<PricingModelMaster>> GetAllAsync();
        Task<IEnumerable<PricingModelMaster>> GetAllActiveAsync();
        Task<bool> ExistsActiveAsync(string modelName);
    }
}
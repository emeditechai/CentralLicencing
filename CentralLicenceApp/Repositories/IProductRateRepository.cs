using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IProductRateRepository
    {
        Task<IEnumerable<ProductRate>> GetAllAsync(int? productId = null);
        Task<ProductRate?> GetByIdAsync(int id);
        Task<bool> RateVariantExistsAsync(int productId, string pricingModel, string billingModel, string billingFrequency, int? ignoreId = null);
        Task<int> CreateAsync(ProductRate productRate);
        Task<bool> UpdateAsync(ProductRate productRate);
        Task<bool> DeleteAsync(int id);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IProductRateDiscountRepository
    {
        Task<IEnumerable<ProductRateDiscountOffer>> GetAllAsync(int? productRateId = null, bool todayOnly = false);
        Task<ProductRateDiscountOffer?> GetByIdAsync(int id);
        Task<bool> PromoCodeExistsAsync(string promoCode, int? ignoreId = null);
        Task<int> CreateAsync(ProductRateDiscountOffer offer);
        Task<bool> UpdateAsync(ProductRateDiscountOffer offer);
        Task<bool> DeleteAsync(int id);
    }
}
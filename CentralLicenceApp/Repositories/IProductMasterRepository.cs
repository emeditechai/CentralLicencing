using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IProductMasterRepository
    {
        Task<IEnumerable<ProductMaster>> GetAllAsync();
        Task<IEnumerable<ProductMaster>> GetAllActiveAsync();
        Task<ProductMaster?> GetByIdAsync(int id);
        Task<bool> ProductCodeExistsAsync(string productCode, int? ignoreId = null);
        Task<int> CreateAsync(ProductMaster product);
        Task<bool> UpdateAsync(ProductMaster product);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITicketSubCategoryRepository
    {
        Task<IEnumerable<TicketSubCategoryMaster>> GetAllAsync();
        Task<IEnumerable<TicketSubCategoryMaster>> GetAllActiveAsync();
        Task<IEnumerable<TicketSubCategoryMaster>> GetByCategoryIdAsync(int categoryId);
        Task<TicketSubCategoryMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(TicketSubCategoryMaster subCategory);
        Task<bool> UpdateAsync(TicketSubCategoryMaster subCategory);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

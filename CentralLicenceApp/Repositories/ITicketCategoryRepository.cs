using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITicketCategoryRepository
    {
        Task<IEnumerable<TicketCategoryMaster>> GetAllAsync();
        Task<IEnumerable<TicketCategoryMaster>> GetAllActiveAsync();
        Task<TicketCategoryMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(TicketCategoryMaster category);
        Task<bool> UpdateAsync(TicketCategoryMaster category);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

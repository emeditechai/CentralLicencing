using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITicketPriorityRepository
    {
        Task<IEnumerable<TicketPriorityMaster>> GetAllAsync();
        Task<IEnumerable<TicketPriorityMaster>> GetAllActiveAsync();
        Task<TicketPriorityMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(TicketPriorityMaster priority);
        Task<bool> UpdateAsync(TicketPriorityMaster priority);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

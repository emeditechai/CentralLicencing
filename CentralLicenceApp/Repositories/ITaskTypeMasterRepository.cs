using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITaskTypeMasterRepository
    {
        Task<IEnumerable<TaskTypeMaster>> GetAllAsync();
        Task<IEnumerable<TaskTypeMaster>> GetAllActiveAsync();
        Task<TaskTypeMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(TaskTypeMaster item);
        Task<bool> UpdateAsync(TaskTypeMaster item);
        Task<bool> ToggleActiveAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsUsedAsync(int id);
    }
}

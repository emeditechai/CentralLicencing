using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITaskCategoryMasterRepository
    {
        Task<IEnumerable<TaskCategoryMaster>> GetAllAsync();
        Task<IEnumerable<TaskCategoryMaster>> GetAllActiveAsync();
        Task<TaskCategoryMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(TaskCategoryMaster item);
        Task<bool> UpdateAsync(TaskCategoryMaster item);
        Task<bool> ToggleActiveAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsUsedAsync(int id);
    }
}

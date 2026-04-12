using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IProjectModuleRepository
    {
        Task<IEnumerable<ProjectModuleMaster>> GetAllAsync();
        Task<IEnumerable<ProjectModuleMaster>> GetAllActiveAsync();
        Task<ProjectModuleMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(ProjectModuleMaster project);
        Task<bool> UpdateAsync(ProjectModuleMaster project);
        Task<bool> ToggleActiveAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsUsedAsync(int id);
    }
}

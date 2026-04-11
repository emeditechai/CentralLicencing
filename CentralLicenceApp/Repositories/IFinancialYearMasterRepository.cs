using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IFinancialYearMasterRepository
    {
        Task<IEnumerable<FinancialYearMaster>> GetAllAsync();
        Task<IEnumerable<FinancialYearMaster>> GetAllActiveAsync();
        Task<FinancialYearMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(FinancialYearMaster fy);
        Task<bool> UpdateAsync(FinancialYearMaster fy);
        Task<bool> ToggleActiveAsync(int id);
        Task<bool> FYCodeExistsAsync(string fyCode, int? excludeId = null);
        Task SyncCurrentFYAsync();
        Task<bool> SetCurrentFYAsync(int id);
        Task<int?> GetCurrentFYIdAsync();
    }
}

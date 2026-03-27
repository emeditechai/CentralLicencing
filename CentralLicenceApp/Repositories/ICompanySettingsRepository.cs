using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ICompanySettingsRepository
    {
        Task<IEnumerable<CompanySetting>> GetAllAsync();
        Task<CompanySetting?> GetByIdAsync(int id);
        Task<CompanySetting?> GetParentCompanyAsync();
        Task<IEnumerable<CompanySetting>> GetParentCompanyOptionsAsync(int? excludeId = null);
        Task<IEnumerable<CompanyTypeMaster>> GetCompanyTypesAsync();
        Task<int> CreateAsync(CompanySetting companySetting);
        Task<bool> UpdateAsync(CompanySetting companySetting);
        Task<bool> DeleteAsync(int id);
        Task<bool> CheckCompanyCodeUniqueAsync(string companyCode, int? excludeId = null);
    }
}
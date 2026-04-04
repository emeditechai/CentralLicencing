using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ITermsConditionTemplateRepository
    {
        Task<IEnumerable<TermsConditionTemplate>> GetAllAsync();
        Task<IEnumerable<TermsConditionTemplate>> GetAllActiveAsync();
        Task<TermsConditionTemplate?> GetByIdAsync(int id);
        Task<bool> IsNameExistsAsync(string name, int? excludeId = null);
        Task<int> CreateAsync(TermsConditionTemplate template);
        Task<bool> UpdateAsync(TermsConditionTemplate template);
        Task<bool> DeleteAsync(int id);
    }
}

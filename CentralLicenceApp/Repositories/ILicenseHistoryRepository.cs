using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ILicenseHistoryRepository
    {
        Task<IEnumerable<LicenseValidationHistory>> GetAllAsync();
        Task<(IEnumerable<LicenseValidationHistory> Items, int TotalCount)> GetPagedAsync(
            string? clientCode, string? validFilter, string? productType, int page, int pageSize);
        Task<IEnumerable<LicenseValidationHistory>> GetByClientCodeAsync(string clientCode);
    }
}

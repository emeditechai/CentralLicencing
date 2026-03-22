using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IClientLicenseRepository
    {
        Task<IEnumerable<ClientAppLicense>> GetAllAsync();
        Task<(IEnumerable<ClientAppLicense> Items, int TotalCount)> GetPagedAsync(string? search, string? status, string? productType, int page, int pageSize);
        Task<ClientAppLicense?> GetByIdAsync(int id);
        Task<ClientAppLicense?> GetByClientCodeAsync(string clientCode);
        Task<int> CreateAsync(ClientAppLicense license);
        Task<bool> UpdateAsync(ClientAppLicense license);
        Task<bool> DeleteAsync(int id);
        Task<DashboardViewModel> GetDashboardStatsAsync(string? productType = null);
        Task<IEnumerable<string>> GetDistinctProductTypesAsync();
        Task<IEnumerable<ClientAppLicense>> GetLicensesExpiringWithinDaysAsync(int days);
        Task<IEnumerable<ClientAppLicense>> GetAmcExpiringWithinDaysAsync(int days);
    }
}

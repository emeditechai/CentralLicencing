using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ISalesCommissionConfigRepository
    {
        // Configuration
        Task<IEnumerable<SalesCommConfigUserRow>> GetAllConfigurationsAsync(string? search, string? typeFilter, string? statusFilter);
        Task<SalesCommissionConfiguration?> GetConfigurationByUserIdAsync(int userId);
        Task<bool> UpsertConfigurationAsync(SalesCommissionConfiguration config);

        // Commission Rules
        Task<IEnumerable<SalesCommissionRule>> GetRulesAsync(int userId);
        Task<SalesCommissionRule?> GetRuleByIdAsync(int id);
        Task<int> AddRuleAsync(SalesCommissionRule rule);
        Task<bool> UpdateRuleAsync(SalesCommissionRule rule);
        Task<bool> DeleteRuleAsync(int id);
    }
}

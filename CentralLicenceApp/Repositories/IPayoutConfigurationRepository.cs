using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IPayoutConfigurationRepository
    {
        // Configuration
        Task<IEnumerable<PayoutConfigUserRow>> GetAllConfigurationsAsync(string? search, string? modelFilter, string? statusFilter);
        Task<PayoutConfiguration?> GetConfigurationByUserIdAsync(int userId);
        Task<bool> UpsertConfigurationAsync(PayoutConfiguration config);

        // Commission Rules
        Task<IEnumerable<PayoutCommissionRule>> GetCommissionRulesAsync(int userId);
        Task<PayoutCommissionRule?> GetCommissionRuleByIdAsync(int id);
        Task<int> AddCommissionRuleAsync(PayoutCommissionRule rule);
        Task<bool> UpdateCommissionRuleAsync(PayoutCommissionRule rule);
        Task<bool> DeleteCommissionRuleAsync(int id);
    }
}

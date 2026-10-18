using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IWhatsAppRepository
    {
        Task EnsureTablesAsync();
        Task<WhatsAppConfiguration?> GetConfigAsync();
        Task<int> SaveConfigAsync(WhatsAppConfiguration config, string user);
        Task UpdateTestResultAsync(int configId, string result);
        Task InsertLogAsync(WhatsAppLog log);
        Task<IEnumerable<WhatsAppLog>> GetRecentLogsAsync(int count);
        Task<IEnumerable<(string Email, string PhoneNumber)>> GetUserPhonesByEmailAsync(IEnumerable<string> emails);
    }
}

using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IClientDetailsRepository
    {
        Task<ClientDetails?> GetByClientCodeAsync(string clientCode);
        Task UpsertAsync(ClientDetails details);
        Task<System.Collections.Generic.IEnumerable<string>> GetClientCodesWithDetailsAsync();
        Task<System.Collections.Generic.IReadOnlyList<ClientPurchasedProduct>> GetPurchasedProductsByClientCodeAsync(string clientCode);
        Task<System.Collections.Generic.IReadOnlyList<SubscriptionInvoiceReminder>> GetSubscriptionRemindersAsync(int daysAhead = 5);
        Task MarkRenewedAsync(int purchasedProductId);
    }
}

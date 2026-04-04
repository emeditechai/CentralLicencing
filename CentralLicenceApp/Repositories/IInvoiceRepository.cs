using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByInvoiceNoAsync(string invoiceNo);
        Task<string> GetNextInvoiceNoAsync();
        Task<decimal> GetPartyOutstandingBalanceAsync(int partyId, int? excludeInvoiceId = null);
        Task<int> CreateAsync(Invoice invoice);
        Task<bool> UpdateAsync(Invoice invoice);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> CancelAsync(int id, string remarks);
        Task<bool> DeleteAsync(int id);
    }
}

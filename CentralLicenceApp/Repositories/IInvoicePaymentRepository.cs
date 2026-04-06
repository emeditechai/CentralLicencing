using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IInvoicePaymentRepository
    {
        Task<IEnumerable<InvoicePayment>> GetAllAsync();
        Task<IEnumerable<InvoicePayment>> GetByInvoiceIdAsync(int invoiceId);
        Task<InvoicePayment?> GetByIdAsync(int id);
        Task<string> GetNextReceiptNoAsync();
        Task<int> CreateAsync(InvoicePayment payment);
        Task<bool> HasActivePaymentsAsync(int invoiceId);
        Task<bool> VoidAsync(int id, string voidedBy, string remarks);
    }
}

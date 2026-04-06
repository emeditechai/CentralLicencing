using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IInvoiceRefundRepository
    {
        Task<IEnumerable<InvoiceRefund>> GetByPaymentIdAsync(int paymentId);
        Task<InvoiceRefund?> GetByIdAsync(int id);
        Task<string> GetNextRefundNoAsync();
        Task<int> CreateAsync(InvoiceRefund refund);
    }
}

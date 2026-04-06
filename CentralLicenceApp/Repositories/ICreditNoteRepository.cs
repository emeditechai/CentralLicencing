using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ICreditNoteRepository
    {
        Task<string> GetNextCreditNoteNoAsync();
        Task<int> CreateAsync(CreditNote creditNote);
        Task<CreditNote?> GetByIdAsync(int id);
        Task<CreditNote?> GetByRefundIdAsync(int refundId);
    }
}

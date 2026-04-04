using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IQuotationRepository
    {
        Task<IEnumerable<Quotation>> GetAllAsync();
        Task<Quotation?> GetByIdAsync(int id);
        Task<string> GetNextQuotationNoAsync();
        Task<int> CreateAsync(Quotation quotation);
        Task<bool> UpdateAsync(Quotation quotation);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<bool> CancelAsync(int id, string remarks);
        Task<bool> DeleteAsync(int id);
    }
}

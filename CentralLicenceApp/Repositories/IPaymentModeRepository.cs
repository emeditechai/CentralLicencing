using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IPaymentModeRepository
    {
        Task<IEnumerable<PaymentMode>> GetAllAsync();
        Task<IEnumerable<PaymentMode>> GetAllActiveAsync();
        Task<PaymentMode?> GetByIdAsync(int id);
        Task<int> CreateAsync(PaymentMode mode);
        Task<bool> UpdateAsync(PaymentMode mode);
        Task<bool> ToggleActiveAsync(int id);
    }
}

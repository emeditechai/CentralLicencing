using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IBankMasterRepository
    {
        Task<IEnumerable<BankMaster>> GetAllAsync();
        Task<BankMaster?> GetByIdAsync(int id);
        Task<BankMaster?> GetPrimaryAsync();
        Task<int> CreateAsync(BankMaster bank);
        Task<bool> UpdateAsync(BankMaster bank);
        Task<bool> DeleteAsync(int id);
    }
}

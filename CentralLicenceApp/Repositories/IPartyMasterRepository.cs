using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IPartyMasterRepository
    {
        Task<IEnumerable<PartyMaster>> GetAllAsync();
        Task<IEnumerable<PartyMaster>> GetAllActiveAsync();
        Task<PartyMaster?> GetByIdAsync(int id);
        Task<int> CreateAsync(PartyMaster party);
        Task<bool> UpdateAsync(PartyMaster party);
        Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id);
        Task<bool> DeleteAsync(int id);
    }
}

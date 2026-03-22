using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IMailConfigRepository
    {
        Task<IEnumerable<MailConfiguration>> GetAllAsync();
        Task<MailConfiguration?> GetByIdAsync(int id);
        Task<MailConfiguration?> GetActiveAsync();
        Task<int> CreateAsync(MailConfiguration config, string createdBy);
        Task UpdateAsync(MailConfiguration config, string updatedBy);
        Task SetActiveAsync(int id, bool isActive);
    }
}

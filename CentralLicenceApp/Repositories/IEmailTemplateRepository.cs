using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IEmailTemplateRepository
    {
        Task<IEnumerable<EmailTemplate>> GetAllAsync();
        Task<EmailTemplate?> GetByIdAsync(int id);
        Task<EmailTemplate?> GetByKeyAsync(string key);
        Task UpdateAsync(EmailTemplate template);
    }
}

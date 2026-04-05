using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IClientLicenseAuditLogRepository
    {
        Task AddAsync(ClientLicenseAuditLog entry);
        Task<(IEnumerable<ClientLicenseAuditLog> Items, int TotalCount)> GetPagedAsync(
            string? search, string? field, int page, int pageSize);
        Task<IEnumerable<ClientLicenseAuditLog>> GetByLicenseIdAsync(int licenseId);
    }
}

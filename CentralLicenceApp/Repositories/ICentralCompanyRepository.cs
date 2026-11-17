using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface ICentralCompanyRepository
    {
        Task EnsureMenuAsync();
        Task<List<CentralCompany>> GetCompaniesAsync(string? search);
        Task<CentralCompany?> GetCompanyAsync(int id);
        Task<List<CentralUser>> GetCompanyUsersAsync(int companyId, string? search);
        Task<CentralUser?> GetUserAsync(long id);
        Task<string?> GetDecryptedConnectionStringAsync(int companyId);
    }
}

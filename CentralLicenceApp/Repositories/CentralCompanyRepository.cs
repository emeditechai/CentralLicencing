using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class CentralCompanyRepository : ICentralCompanyRepository
    {
        private readonly string _connectionString;

        public CentralCompanyRepository(string connectionString) { _connectionString = connectionString; }

        private SqlConnection CreateConnection() => new(_connectionString);

        // ConnectionString / DecryptedConnectionString are deliberately never selected.
        private const string CompanySelect = @"
            SELECT c.ID, c.CompanyCode, c.CompanyName, c.ApplicationCompanyID, c.InstanceKey, c.CreatedDate, c.ModifiedDate, c.ApiUrl, c.WebUrl, c.ValidFrom, c.ValidTo,
                   CAST(CASE WHEN NULLIF(LTRIM(RTRIM(c.ConnectionString)), '') IS NULL THEN 0 ELSE 1 END AS BIT) AS HasConnectionString,
                   (SELECT COUNT(*) FROM Central_Users u WHERE u.CompanyID = c.ID AND u.InstanceKey = c.InstanceKey) AS UserCount,
                   (SELECT MAX(u.CreatedDate) FROM Central_Users u WHERE u.CompanyID = c.ID AND u.InstanceKey = c.InstanceKey) AS LastUserAddedDate
            FROM Central_Company c";

        public async Task EnsureMenuAsync()
        {
            var path = Helpers.SqlScriptPath.Resolve("088_AddCentralCompanyMenu.sql");
            if (!File.Exists(path)) return;
            using var conn = CreateConnection();
            var sql = (await File.ReadAllTextAsync(path)).Replace("\r\n", "\n");
            foreach (var batch in sql.Split("\nGO\n", StringSplitOptions.RemoveEmptyEntries))
                if (!string.IsNullOrWhiteSpace(batch)) await conn.ExecuteAsync(batch);
        }

        public async Task<List<CentralCompany>> GetCompaniesAsync(string? search)
        {
            using var conn = CreateConnection();
            var sql = CompanySelect;
            if (!string.IsNullOrWhiteSpace(search))
                sql += " WHERE c.CompanyName LIKE @S OR c.CompanyCode LIKE @S OR c.ApiUrl LIKE @S OR c.WebUrl LIKE @S";
            return (await conn.QueryAsync<CentralCompany>(sql + " ORDER BY c.CompanyName", new { S = $"%{search?.Trim()}%" })).ToList();
        }

        public async Task<CentralCompany?> GetCompanyAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<CentralCompany>(CompanySelect + " WHERE c.ID = @id", new { id });
        }

        // Users belong to a company by CompanyID and InstanceKey together (FK_Central_Users_Company_Instance).
        public async Task<List<CentralUser>> GetCompanyUsersAsync(int companyId, string? search)
        {
            using var conn = CreateConnection();
            var sql = @"SELECT u.ID, u.Username, u.CompanyID, u.ApplicationUserID, u.CreatedDate, u.ModifiedDate, u.InstanceKey,
                               c.CompanyCode, c.CompanyName
                        FROM Central_Users u
                        INNER JOIN Central_Company c ON c.ID = u.CompanyID AND c.InstanceKey = u.InstanceKey
                        WHERE c.ID = @companyId";
            if (!string.IsNullOrWhiteSpace(search)) sql += " AND u.Username LIKE @S";
            return (await conn.QueryAsync<CentralUser>(sql + " ORDER BY u.Username", new { companyId, S = $"%{search?.Trim()}%" })).ToList();
        }

        // Read only when an Administrator asks to reveal it; never part of the page model.
        public async Task<string?> GetDecryptedConnectionStringAsync(int companyId)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<string?>("SELECT DecryptedConnectionString FROM Central_Company WHERE ID = @companyId", new { companyId });
        }

        public async Task<CentralUser?> GetUserAsync(long id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<CentralUser>(@"
                SELECT u.ID, u.Username, u.CompanyID, u.ApplicationUserID, u.CreatedDate, u.ModifiedDate, u.InstanceKey,
                       c.CompanyCode, c.CompanyName
                FROM Central_Users u
                LEFT JOIN Central_Company c ON c.ID = u.CompanyID AND c.InstanceKey = u.InstanceKey
                WHERE u.ID = @id", new { id });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class CompanySettingsRepository : ICompanySettingsRepository
    {
        private readonly string _connectionString;

        public CompanySettingsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<CompanySetting>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<CompanySetting>(@"
                SELECT c.*, t.TypeName AS CompanyTypeName,
                       parent.CompanyName AS ParentCompanyName,
                       parent.City AS ParentCompanyCity
                FROM CompanySettings c
                INNER JOIN CompanyTypeMaster t ON c.CompanyTypeId = t.Id
                LEFT JOIN CompanySettings parent ON c.ParentCompanyId = parent.Id
                ORDER BY c.CreatedAt DESC");
        }

        public async Task<CompanySetting?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<CompanySetting>(@"
                SELECT c.*, t.TypeName AS CompanyTypeName,
                       parent.CompanyName AS ParentCompanyName,
                       parent.City AS ParentCompanyCity
                FROM CompanySettings c
                INNER JOIN CompanyTypeMaster t ON c.CompanyTypeId = t.Id
                LEFT JOIN CompanySettings parent ON c.ParentCompanyId = parent.Id
                WHERE c.Id = @Id", new { Id = id });
        }

        public async Task<CompanySetting?> GetParentCompanyAsync()
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<CompanySetting>(@"
                SELECT TOP 1 c.*, t.TypeName AS CompanyTypeName,
                       parent.CompanyName AS ParentCompanyName,
                       parent.City AS ParentCompanyCity
                FROM CompanySettings c
                INNER JOIN CompanyTypeMaster t ON c.CompanyTypeId = t.Id
                LEFT JOIN CompanySettings parent ON c.ParentCompanyId = parent.Id
                WHERE c.IsParentCompany = 1 AND c.IsActive = 1
                ORDER BY c.Id ASC");
        }

        public async Task<IEnumerable<CompanySetting>> GetParentCompanyOptionsAsync(int? excludeId = null)
        {
            using var conn = CreateConnection();
            var options = await conn.QueryAsync<CompanySetting>(@"
                SELECT Id, CompanyName, City, IsActive
                FROM CompanySettings
                WHERE (@ExcludeId IS NULL OR Id <> @ExcludeId)
                ORDER BY CompanyName ASC, City ASC, Id ASC", new { ExcludeId = excludeId });

            return options
                .Where(company => company.Id > 0)
                .ToList();
        }

        public async Task<IEnumerable<CompanyTypeMaster>> GetCompanyTypesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<CompanyTypeMaster>(
                "SELECT * FROM CompanyTypeMaster WHERE IsActive = 1 ORDER BY TypeName");
        }

        public async Task<int> CreateAsync(CompanySetting companySetting)
        {
            using var conn = CreateConnection();
            companySetting.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO CompanySettings
                    (CompanyCode, CompanyTypeId, CompanyName, Country, State, District, City, Address, Website, EmailId, ContactNo, Pincode, GSTCode, PANCard, ParentCompanyId, IsParentCompany, IsExpenseEmailNotificationRequired, CompanyLogoPath, IsActive, CreatedAt)
                VALUES
                    (@CompanyCode, @CompanyTypeId, @CompanyName, @Country, @State, @District, @City, @Address, @Website, @EmailId, @ContactNo, @Pincode, @GSTCode, @PANCard, @ParentCompanyId, @IsParentCompany, @IsExpenseEmailNotificationRequired, @CompanyLogoPath, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", companySetting);
        }

        public async Task<bool> UpdateAsync(CompanySetting companySetting)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE CompanySettings SET
                    CompanyCode = @CompanyCode,
                    CompanyTypeId = @CompanyTypeId,
                    CompanyName = @CompanyName,
                    Country = @Country,
                    State = @State,
                    District = @District,
                    City = @City,
                    Address = @Address,
                    Website = @Website,
                    EmailId = @EmailId,
                    ContactNo = @ContactNo,
                    Pincode = @Pincode,
                    GSTCode = @GSTCode,
                    PANCard = @PANCard,
                    ParentCompanyId = @ParentCompanyId,
                    IsParentCompany = @IsParentCompany,
                    IsExpenseEmailNotificationRequired = @IsExpenseEmailNotificationRequired,
                    CompanyLogoPath = @CompanyLogoPath,
                    IsActive = @IsActive
                WHERE Id = @Id", companySetting) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync("DELETE FROM CompanySettings WHERE Id = @Id", new { Id = id }) > 0;
        }

        public async Task<bool> CheckCompanyCodeUniqueAsync(string companyCode, int? excludeId = null)
        {
            using var conn = CreateConnection();
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM CompanySettings WHERE CompanyCode = @CompanyCode AND Id <> @ExcludeId"
                : "SELECT COUNT(1) FROM CompanySettings WHERE CompanyCode = @CompanyCode";

            var count = await conn.ExecuteScalarAsync<int>(sql, new { CompanyCode = companyCode, ExcludeId = excludeId ?? 0 });
            return count == 0;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TermsConditionTemplateRepository : ITermsConditionTemplateRepository
    {
        private readonly string _connectionString;

        public TermsConditionTemplateRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TermsConditionTemplate>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TermsConditionTemplate>(
                "SELECT * FROM TermsConditionTemplate ORDER BY TermsName");
        }

        public async Task<IEnumerable<TermsConditionTemplate>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TermsConditionTemplate>(
                "SELECT * FROM TermsConditionTemplate WHERE IsActive = 1 ORDER BY TermsName");
        }

        public async Task<TermsConditionTemplate?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TermsConditionTemplate>(
                "SELECT * FROM TermsConditionTemplate WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM TermsConditionTemplate WHERE TermsName = @Name AND (@ExcludeId IS NULL OR Id <> @ExcludeId)",
                new { Name = name.Trim(), ExcludeId = excludeId }) > 0;
        }

        public async Task<int> CreateAsync(TermsConditionTemplate template)
        {
            using var conn = CreateConnection();
            template.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO TermsConditionTemplate (TermsName, Description, IsActive, CreatedAt)
                VALUES (@TermsName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", template);
        }

        public async Task<bool> UpdateAsync(TermsConditionTemplate template)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE TermsConditionTemplate SET
                    TermsName   = @TermsName,
                    Description = @Description,
                    IsActive    = @IsActive
                WHERE Id = @Id", template) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TermsConditionTemplate WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class EmailTemplateRepository : IEmailTemplateRepository
    {
        private readonly string _connectionString;

        public EmailTemplateRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<EmailTemplate>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmailTemplate>(
                "SELECT * FROM tbl_centralemailtemplates ORDER BY Id");
        }

        public async Task<EmailTemplate?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmailTemplate>(
                "SELECT * FROM tbl_centralemailtemplates WHERE Id = @Id", new { Id = id });
        }

        public async Task<EmailTemplate?> GetByKeyAsync(string key)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmailTemplate>(
                "SELECT * FROM tbl_centralemailtemplates WHERE TemplateKey = @Key AND IsActive = 1",
                new { Key = key });
        }

        public async Task UpdateAsync(EmailTemplate template)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE tbl_centralemailtemplates SET
                    TemplateName = @TemplateName,
                    Subject      = @Subject,
                    Body         = @Body,
                    IsActive     = @IsActive,
                    UpdatedAt    = GETDATE()
                WHERE Id = @Id", template);
        }
    }
}

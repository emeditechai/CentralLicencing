using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TicketCategoryRepository : ITicketCategoryRepository
    {
        private readonly string _connectionString;

        public TicketCategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TicketCategoryMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketCategoryMaster>(
                "SELECT * FROM TicketCategoryMaster ORDER BY CategoryName");
        }

        public async Task<IEnumerable<TicketCategoryMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketCategoryMaster>(
                "SELECT * FROM TicketCategoryMaster WHERE IsActive = 1 ORDER BY CategoryName");
        }

        public async Task<TicketCategoryMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TicketCategoryMaster>(
                "SELECT * FROM TicketCategoryMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(TicketCategoryMaster category)
        {
            using var conn = CreateConnection();
            category.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
                VALUES (@CategoryName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, category);
        }

        public async Task<bool> UpdateAsync(TicketCategoryMaster category)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE TicketCategoryMaster SET
                    CategoryName = @CategoryName,
                    Description  = @Description,
                    IsActive     = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, category) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM HelpDeskTicket WHERE CategoryId = @Id", new { Id = id });

            return count == 0
                ? (true, null)
                : (false, $"This category cannot be deleted because {count} ticket(s) reference it.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TicketCategoryMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

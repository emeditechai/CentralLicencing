using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TicketPriorityRepository : ITicketPriorityRepository
    {
        private readonly string _connectionString;

        public TicketPriorityRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TicketPriorityMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketPriorityMaster>(
                "SELECT * FROM TicketPriorityMaster ORDER BY SortOrder");
        }

        public async Task<IEnumerable<TicketPriorityMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketPriorityMaster>(
                "SELECT * FROM TicketPriorityMaster WHERE IsActive = 1 ORDER BY SortOrder");
        }

        public async Task<TicketPriorityMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TicketPriorityMaster>(
                "SELECT * FROM TicketPriorityMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(TicketPriorityMaster priority)
        {
            using var conn = CreateConnection();
            priority.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketPriorityMaster (PriorityName, ColorCode, SortOrder, SlaResponseHours, SlaResolutionHours, IsActive, CreatedAt)
                VALUES (@PriorityName, @ColorCode, @SortOrder, @SlaResponseHours, @SlaResolutionHours, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, priority);
        }

        public async Task<bool> UpdateAsync(TicketPriorityMaster priority)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE TicketPriorityMaster SET
                    PriorityName      = @PriorityName,
                    ColorCode         = @ColorCode,
                    SortOrder         = @SortOrder,
                    SlaResponseHours  = @SlaResponseHours,
                    SlaResolutionHours = @SlaResolutionHours,
                    IsActive          = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, priority) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM HelpDeskTicket WHERE PriorityId = @Id", new { Id = id });

            return count == 0
                ? (true, null)
                : (false, $"This priority cannot be deleted because {count} ticket(s) reference it.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TicketPriorityMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

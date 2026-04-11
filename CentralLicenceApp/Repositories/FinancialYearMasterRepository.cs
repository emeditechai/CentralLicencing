using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class FinancialYearMasterRepository : IFinancialYearMasterRepository
    {
        private readonly string _connectionString;

        public FinancialYearMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<FinancialYearMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<FinancialYearMaster>(
                "SELECT * FROM FinancialYearMaster ORDER BY StartDate DESC");
        }

        public async Task<IEnumerable<FinancialYearMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<FinancialYearMaster>(
                "SELECT * FROM FinancialYearMaster WHERE IsActive = 1 ORDER BY StartDate DESC");
        }

        public async Task<FinancialYearMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<FinancialYearMaster>(
                "SELECT * FROM FinancialYearMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(FinancialYearMaster fy)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO FinancialYearMaster (StartDate, EndDate, FYCode, IsActive, CreatedAt, UpdatedAt)
                VALUES (@StartDate, @EndDate, @FYCode, @IsActive, GETDATE(), GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);", fy);
        }

        public async Task<bool> UpdateAsync(FinancialYearMaster fy)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE FinancialYearMaster
                SET StartDate = @StartDate,
                    EndDate   = @EndDate,
                    FYCode    = @FYCode,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id", fy);
            return rows > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE FinancialYearMaster
                SET IsActive  = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
                    UpdatedAt = GETDATE()
                WHERE Id = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<bool> FYCodeExistsAsync(string fyCode, int? excludeId = null)
        {
            using var conn = CreateConnection();
            var sql = excludeId.HasValue
                ? "SELECT COUNT(1) FROM FinancialYearMaster WHERE FYCode = @FYCode AND Id <> @ExcludeId"
                : "SELECT COUNT(1) FROM FinancialYearMaster WHERE FYCode = @FYCode";
            var count = await conn.ExecuteScalarAsync<int>(sql, new { FYCode = fyCode, ExcludeId = excludeId ?? 0 });
            return count > 0;
        }

        /// <summary>
        /// Automatically sets IsCurrentFY = 1 for the FY where today falls between StartDate and EndDate,
        /// and clears it from all others. This ensures the flag cycles each year without manual intervention.
        /// </summary>
        public async Task SyncCurrentFYAsync()
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                UPDATE FinancialYearMaster
                SET IsCurrentFY = CASE
                        WHEN IsActive = 1 AND CAST(GETDATE() AS DATE) >= StartDate AND CAST(GETDATE() AS DATE) <= EndDate THEN 1
                        ELSE 0
                    END,
                    UpdatedAt = GETDATE()");
        }

        /// <summary>
        /// Manually mark a specific FY as current (clears all others first).
        /// </summary>
        public async Task<bool> SetCurrentFYAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE FinancialYearMaster SET IsCurrentFY = 0, UpdatedAt = GETDATE();
                UPDATE FinancialYearMaster SET IsCurrentFY = 1, UpdatedAt = GETDATE() WHERE Id = @Id;",
                new { Id = id });
            return rows > 0;
        }

        public async Task<int?> GetCurrentFYIdAsync()
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int?>(@"
                SELECT TOP 1 Id FROM FinancialYearMaster
                WHERE IsActive = 1
                  AND CAST(GETDATE() AS DATE) >= StartDate
                  AND CAST(GETDATE() AS DATE) <= EndDate");
        }
    }
}

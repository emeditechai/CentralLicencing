using System.Data;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PayoutReportRepository : IPayoutReportRepository
    {
        private readonly string _connStr;
        public PayoutReportRepository(string connStr) => _connStr = connStr;

        public async Task<IEnumerable<PayoutSummaryRow>> GetSummaryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutSummaryRow>(
                "sp_PayoutSummaryReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PayoutDetailRow>> GetDetailReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int? batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutDetailRow>(
                "sp_PayoutDetailReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, BatchId = batchId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<PayoutHistoryRow>> GetHistoryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, string? status)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutHistoryRow>(
                "sp_PayoutHistoryReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Status = status },
                commandType: CommandType.StoredProcedure);
        }

        public async Task EnsureStoredProceduresAsync()
        {
            var assembly = typeof(PayoutReportRepository).Assembly;
            var resourcePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "069_PayoutReportProcedures.sql");

            // Fallback to relative from current directory
            if (!File.Exists(resourcePath))
                resourcePath = Path.Combine(Directory.GetCurrentDirectory(), "SqlScripts", "069_PayoutReportProcedures.sql");

            if (!File.Exists(resourcePath)) return;

            var sql = await File.ReadAllTextAsync(resourcePath);
            var batches = sql.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n", "\r\nGO\n" },
                StringSplitOptions.RemoveEmptyEntries);

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            foreach (var batch in batches)
            {
                var trimmed = batch.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                await conn.ExecuteAsync(trimmed);
            }
        }
    }
}

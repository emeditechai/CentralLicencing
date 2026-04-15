using System.Data;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class SalesCommissionReportRepository : ISalesCommissionReportRepository
    {
        private readonly string _connStr;
        public SalesCommissionReportRepository(string connStr) => _connStr = connStr;

        public async Task<IEnumerable<SalesCommSummaryRow>> GetSummaryReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommSummaryRow>(
                "sp_SalesCommissionSummaryReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SalesCommDetailRow>> GetDetailReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, int? batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommDetailRow>(
                "sp_SalesCommissionDetailReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, BatchId = batchId },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<SalesCommHistoryRow>> GetHistoryReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, string? status)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommHistoryRow>(
                "sp_SalesCommissionHistoryReport",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Status = status },
                commandType: CommandType.StoredProcedure);
        }
    }
}

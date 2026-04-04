using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models.Reports;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IReadOnlyList<ClientDetailsReportRow>> GetClientDetailsReportAsync(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<ClientDetailsReportRow>(
                "dbo.usp_Report_ClientDetails",
                new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    ProductType = string.IsNullOrWhiteSpace(productType) ? null : productType.Trim()
                },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task<(IReadOnlyList<ExpenseReportRow> Items, int TotalCount)> GetExpenseReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int page, int pageSize)
        {
            using var conn = CreateConnection();
            var items = (await conn.QueryAsync<ExpenseReportRow>(
                "dbo.usp_Report_ExpenseDetails",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure)).ToList();

            var totalCount = items.FirstOrDefault()?.TotalCount ?? 0;
            return (items, totalCount);
        }

        public async Task<IReadOnlyList<ExpenseReportRow>> GetAllExpenseReportAsync(DateTime? fromDate, DateTime? toDate, int? userId)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<ExpenseReportRow>(
                "dbo.usp_Report_ExpenseDetails",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Page = 1, PageSize = int.MaxValue },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task<(IReadOnlyList<SettlementReportRow> Items, int TotalCount)> GetSettlementReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int page, int pageSize)
        {
            using var conn = CreateConnection();
            var items = (await conn.QueryAsync<SettlementReportRow>(
                "dbo.usp_Report_SettlementDetails",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure)).ToList();

            var totalCount = items.FirstOrDefault()?.TotalCount ?? 0;
            return (items, totalCount);
        }

        public async Task<IReadOnlyList<SettlementReportRow>> GetAllSettlementReportAsync(DateTime? fromDate, DateTime? toDate, int? userId)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<SettlementReportRow>(
                "dbo.usp_Report_SettlementDetails",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, Page = 1, PageSize = int.MaxValue },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }
    }
}
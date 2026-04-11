using System.Data;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TicketReportRepository : ITicketReportRepository
    {
        private readonly string _connectionString;

        public TicketReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SqlConnection CreateConnection() => new(_connectionString);

        public async Task<TicketReportDashboardViewModel> GetDashboardAsync(DateTime? fromDate, DateTime? toDate)
        {
            using var conn = CreateConnection();
            var p = new { FromDate = fromDate, ToDate = toDate };

            // KPIs
            var kpi = await conn.QuerySingleAsync<TicketReportDashboardViewModel>(
                "dbo.usp_TicketReport_Dashboard", p, commandType: CommandType.StoredProcedure);

            kpi.FromDate = fromDate;
            kpi.ToDate = toDate;

            // Status distribution
            kpi.StatusDistribution = (await conn.QueryAsync<StatusDistributionRow>(
                "dbo.usp_TicketReport_StatusDistribution", p, commandType: CommandType.StoredProcedure)).ToList();

            // Category distribution
            kpi.CategoryDistribution = (await conn.QueryAsync<CategoryDistributionRow>(
                "dbo.usp_TicketReport_CategoryDistribution", p, commandType: CommandType.StoredProcedure)).ToList();

            // Priority distribution
            kpi.PriorityDistribution = (await conn.QueryAsync<PriorityDistributionRow>(
                "dbo.usp_TicketReport_PriorityDistribution", p, commandType: CommandType.StoredProcedure)).ToList();

            // Daily trend
            kpi.DailyTrend = (await conn.QueryAsync<DailyTrendRow>(
                "dbo.usp_TicketReport_DailyTrend", p, commandType: CommandType.StoredProcedure)).ToList();

            return kpi;
        }

        public async Task<(List<AgentPerformanceRow> Items, int TotalCount)> GetAgentPerformanceAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize, int? agentId = null)
        {
            using var conn = CreateConnection();
            var rows = (await conn.QueryAsync<AgentPerformanceRow>(
                "dbo.usp_TicketReport_AgentPerformance",
                new { FromDate = fromDate, ToDate = toDate, Page = page, PageSize = pageSize, AgentId = agentId },
                commandType: CommandType.StoredProcedure)).ToList();
            var totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;
            return (rows, totalCount);
        }

        public async Task<(List<SlaComplianceRow> Items, int TotalCount)> GetSlaComplianceAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize)
        {
            using var conn = CreateConnection();
            var rows = (await conn.QueryAsync<SlaComplianceRow>(
                "dbo.usp_TicketReport_SlaCompliance",
                new { FromDate = fromDate, ToDate = toDate, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure)).ToList();
            var totalCount = rows.FirstOrDefault()?.TotalCount ?? 0;
            return (rows, totalCount);
        }

        public async Task<SlaComplianceSummary> GetSlaComplianceSummaryAsync(DateTime? fromDate, DateTime? toDate)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleAsync<SlaComplianceSummary>(
                "dbo.usp_TicketReport_SlaComplianceSummary",
                new { FromDate = fromDate, ToDate = toDate },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<List<AgentPerformanceRow>> GetAllAgentPerformanceAsync(DateTime? fromDate, DateTime? toDate, int? agentId = null)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<AgentPerformanceRow>(
                "dbo.usp_TicketReport_AgentPerformance",
                new { FromDate = fromDate, ToDate = toDate, Page = 1, PageSize = 999999, AgentId = agentId },
                commandType: CommandType.StoredProcedure)).ToList();
        }

        public async Task<List<SlaComplianceRow>> GetAllSlaComplianceAsync(DateTime? fromDate, DateTime? toDate)
        {
            using var conn = CreateConnection();
            return (await conn.QueryAsync<SlaComplianceRow>(
                "dbo.usp_TicketReport_SlaCompliance",
                new { FromDate = fromDate, ToDate = toDate, Page = 1, PageSize = 999999 },
                commandType: CommandType.StoredProcedure)).ToList();
        }
    }
}

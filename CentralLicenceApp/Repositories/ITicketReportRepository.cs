using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ITicketReportRepository
    {
        Task<TicketReportDashboardViewModel> GetDashboardAsync(DateTime? fromDate, DateTime? toDate);
        Task<(List<AgentPerformanceRow> Items, int TotalCount)> GetAgentPerformanceAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize, int? agentId = null);
        Task<(List<SlaComplianceRow> Items, int TotalCount)> GetSlaComplianceAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<SlaComplianceSummary> GetSlaComplianceSummaryAsync(DateTime? fromDate, DateTime? toDate);

        // Non-paginated for export
        Task<List<AgentPerformanceRow>> GetAllAgentPerformanceAsync(DateTime? fromDate, DateTime? toDate, int? agentId = null);
        Task<List<SlaComplianceRow>> GetAllSlaComplianceAsync(DateTime? fromDate, DateTime? toDate);
    }

    public class SlaComplianceSummary
    {
        public int TotalTickets { get; set; }
        public int ResponseSlaMetCount { get; set; }
        public int ResponseSlaBreachedCount { get; set; }
        public int ResolutionSlaMetCount { get; set; }
        public int ResolutionSlaBreachedCount { get; set; }
    }
}

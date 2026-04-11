namespace CentralLicenceApp.Models.ViewModels
{
    // ── Dashboard / Summary KPIs ──
    public class TicketReportDashboardViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // KPI cards
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int WaitingTickets { get; set; }
        public double AvgResponseTimeHours { get; set; }
        public double AvgResolutionTimeHours { get; set; }

        // Status distribution
        public List<StatusDistributionRow> StatusDistribution { get; set; } = new();
        // Category distribution
        public List<CategoryDistributionRow> CategoryDistribution { get; set; } = new();
        // Priority distribution
        public List<PriorityDistributionRow> PriorityDistribution { get; set; } = new();
        // Daily trend
        public List<DailyTrendRow> DailyTrend { get; set; } = new();
    }

    public class StatusDistributionRow
    {
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

    public class CategoryDistributionRow
    {
        public string CategoryName { get; set; } = "";
        public int Count { get; set; }
    }

    public class PriorityDistributionRow
    {
        public string PriorityName { get; set; } = "";
        public string? ColorCode { get; set; }
        public int Count { get; set; }
    }

    public class DailyTrendRow
    {
        public DateTime TicketDate { get; set; }
        public int CreatedCount { get; set; }
        public int ResolvedCount { get; set; }
    }

    // ── Agent Performance Report ──
    public class AgentPerformanceReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<AgentPerformanceRow> Agents { get; set; } = new();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class AgentPerformanceRow
    {
        public int AgentId { get; set; }
        public string AgentName { get; set; } = "";
        public int TotalAssigned { get; set; }
        public int Resolved { get; set; }
        public int Closed { get; set; }
        public int Open { get; set; }
        public int InProgress { get; set; }
        public double AvgResponseTimeHours { get; set; }
        public double AvgResolutionTimeHours { get; set; }
        public double ResolutionRate { get; set; }
        public int TotalCount { get; set; }
    }

    // ── SLA Compliance Report ──
    public class SlaComplianceReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<SlaComplianceRow> Rows { get; set; } = new();

        // Summary
        public int TotalTickets { get; set; }
        public int ResponseSlaMetCount { get; set; }
        public int ResponseSlaBreachedCount { get; set; }
        public int ResolutionSlaMetCount { get; set; }
        public int ResolutionSlaBreachedCount { get; set; }
        public double ResponseSlaCompliancePercent { get; set; }
        public double ResolutionSlaCompliancePercent { get; set; }

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)TotalTickets / PageSize);
    }

    public class SlaComplianceRow
    {
        public int TicketId { get; set; }
        public string TicketNumber { get; set; } = "";
        public string Subject { get; set; } = "";
        public string PriorityName { get; set; } = "";
        public string? PriorityColor { get; set; }
        public string Status { get; set; } = "";
        public string CreatedByName { get; set; } = "";
        public string? AssignedToName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public double SlaResponseHours { get; set; }
        public double SlaResolutionHours { get; set; }
        public double? ActualResponseHours { get; set; }
        public double? ActualResolutionHours { get; set; }
        public string ResponseSlaStatus { get; set; } = ""; // Met, Breached, Pending
        public string ResolutionSlaStatus { get; set; } = "";
        public int TotalCount { get; set; }
    }
}

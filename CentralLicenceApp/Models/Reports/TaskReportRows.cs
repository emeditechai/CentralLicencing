using System;

namespace CentralLicenceApp.Models.Reports
{
    public class TimesheetReportRow
    {
        public int Id { get; set; }
        public DateTime LogDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskTypeName { get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;
        public int TimeSpentMinutes { get; set; }
        public string? Remarks { get; set; }
        public string ProjectModuleName { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }

    public class EmployeeProductivityRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalMinutes { get; set; }
        public int DevMinutes { get; set; }
        public int SupportMinutes { get; set; }
        public decimal CompletionRate { get; set; }
        public int AvgMinutesPerTask { get; set; }
    }

    public class ProjectEffortRow
    {
        public int ProjectModuleId { get; set; }
        public string ProjectModuleName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int TotalMinutes { get; set; }
        public int DevMinutes { get; set; }
        public int SupportMinutes { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
    }
}

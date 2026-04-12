using System;
using System.Collections.Generic;
using System.Linq;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Models.ViewModels
{
    // ── Timesheet Report ──
    public class TimesheetReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public int? TaskTypeFilter { get; set; }
        public bool IsAdminView { get; set; }

        public List<TimesheetReportRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public List<TaskTypeMaster> TaskTypes { get; set; } = new();

        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Summary
        public int TotalEntries => TotalCount;
        public int TotalMinutes => Items.Sum(x => x.TimeSpentMinutes);
    }

    // ── Employee Productivity Report ──
    public class EmployeeProductivityReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public bool IsAdminView { get; set; }

        public List<EmployeeProductivityRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();

        // Summary
        public int GrandTotalTasks => Items.Sum(x => x.TotalTasks);
        public int GrandTotalMinutes => Items.Sum(x => x.TotalMinutes);
        public decimal AvgCompletionRate => Items.Any() ? Math.Round(Items.Average(x => x.CompletionRate), 1) : 0;
    }

    // ── Project / Module Effort Report ──
    public class ProjectEffortReportViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public int? ProjectModuleFilter { get; set; }
        public bool IsAdminView { get; set; }

        public List<ProjectEffortRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public List<ProjectModuleMaster> ProjectModules { get; set; } = new();

        // Summary
        public int GrandTotalTasks => Items.Sum(x => x.TaskCount);
        public int GrandTotalMinutes => Items.Sum(x => x.TotalMinutes);
        public int UniqueProjects => Items.Select(x => x.ProjectModuleId).Distinct().Count();
    }
}

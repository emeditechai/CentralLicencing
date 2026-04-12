using CentralLicenceApp.Models;

namespace CentralLicenceApp.Models.ViewModels
{
    public class DailyTaskLogIndexViewModel
    {
        public List<DailyTaskLog> Tasks { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? TaskTypeFilter { get; set; }
        public int? CategoryFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int? UserFilter { get; set; }
        public string? SearchTerm { get; set; }

        // Lookups
        public List<TaskTypeMaster> TaskTypes { get; set; } = new();
        public List<TaskCategoryMaster> TaskCategories { get; set; } = new();
        public List<UserMaster> TeamMembers { get; set; } = new();

        // Summary
        public int TotalTasks { get; set; }
        public int TotalMinutes { get; set; }
        public int DevMinutes { get; set; }
        public int SupportMinutes { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }

        public bool IsManagerView { get; set; }
        public bool CanCreateTask { get; set; }
        public bool CanEditDelete { get; set; }
    }

    public class DailyTaskLogFormViewModel
    {
        public DailyTaskLog Task { get; set; } = new();
        public List<TaskTypeMaster> TaskTypes { get; set; } = new();
        public List<TaskCategoryMaster> TaskCategories { get; set; } = new();
        public List<ProjectModuleMaster> Projects { get; set; } = new();
        public List<UserMaster> AssignableUsers { get; set; } = new();
        public bool IsEdit { get; set; }
    }

    public class TaskLogDashboardViewModel
    {
        public int TotalTasks { get; set; }
        public int TotalMinutes { get; set; }
        public int DevMinutes { get; set; }
        public int SupportMinutes { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public List<UserTaskSummary> TeamSummary { get; set; } = new();
    }

    public class UserTaskSummary
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int TotalMinutes { get; set; }
        public int DevMinutes { get; set; }
        public int SupportMinutes { get; set; }
    }
}

namespace CentralLicenceApp.Models
{
    public class DailyTaskLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime TaskDate { get; set; } = DateTime.Today;
        public int TaskTypeId { get; set; }
        public int TaskCategoryId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TicketId { get; set; }
        public int? ProjectModuleId { get; set; }
        public int TimeSpentMinutes { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Computed from TaskTimeLog
        public int TotalTimeSpentMinutes { get; set; }
        public DateTime? LastLogDate { get; set; }
        public int TimeLogCount { get; set; }

        // Navigation / display helpers
        public string UserName { get; set; } = string.Empty;
        public string TaskTypeName { get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string? TicketNumber { get; set; }
        public string? TicketSubject { get; set; }
        public string? ProjectModuleName { get; set; }
        public string? AssignedToUserName { get; set; }
    }

    public class TaskTimeLog
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public DateTime LogDate { get; set; } = DateTime.Today;
        public int TimeSpentMinutes { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public string UserName { get; set; } = string.Empty;
    }

    public class TaskTypeMaster
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class TaskCategoryMaster
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class ProjectModuleMaster
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class DailyTaskApproval
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ApprovedBy { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Remarks { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ApprovedByName { get; set; } = string.Empty;
    }
}

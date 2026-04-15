using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    // ── Payout Configuration (1 per user) ──────────────────────────
    public class PayoutConfiguration
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("^(Hourly|Commission)$", ErrorMessage = "Payout model must be Hourly or Commission.")]
        public string PayoutModel { get; set; } = "Hourly";

        [Range(0.01, 999999.99, ErrorMessage = "Hourly rate must be greater than zero.")]
        public decimal? HourlyRate { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Default commission must be greater than zero.")]
        public decimal? DefaultCommissionAmount { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Display
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
    }

    // ── Commission Rule (0..N per user, hierarchical) ──────────────
    public class PayoutCommissionRule
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? TaskTypeId { get; set; }
        public int? TaskCategoryId { get; set; }
        public int? ProjectModuleId { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        public int Priority { get; set; }  // Auto-computed: Project=30, Category=20, Type=10, Default=0

        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display
        public string? UserName { get; set; }
        public string? TaskTypeName { get; set; }
        public string? TaskCategoryName { get; set; }
        public string? ProjectModuleName { get; set; }

        public string RateSourceDisplay =>
            ProjectModuleId.HasValue ? $"Project: {ProjectModuleName}"
            : TaskCategoryId.HasValue ? $"Category: {TaskCategoryName}"
            : TaskTypeId.HasValue ? $"Type: {TaskTypeName}"
            : "Default";
    }

    // ── Payout Batch (1 per user per generated period) ─────────────
    public class PayoutBatch
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public string PayoutModel { get; set; } = string.Empty;      // snapshot
        public decimal? HourlyRateSnapshot { get; set; }
        public int TotalMinutes { get; set; }
        public int TotalTasks { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Draft";

        public string? Remarks { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int GeneratedById { get; set; }

        // Settlement fields
        public decimal? SettlementAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public int? SettledById { get; set; }
        public string? SettlementMode { get; set; }
        public string? SettlementReferenceNo { get; set; }
        public int? SettlementBankId { get; set; }
        public string? SettlementRemarks { get; set; }

        // Display
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? GeneratedByName { get; set; }
        public string? SettledByName { get; set; }
        public string? BankName { get; set; }

        // Computed
        public string FormattedTotalTime
        {
            get
            {
                if (TotalMinutes == 0) return "0m";
                var h = TotalMinutes / 60;
                var m = TotalMinutes % 60;
                if (h == 0) return $"{m}m";
                if (m == 0) return $"{h}h";
                return $"{h}h {m}m";
            }
        }

        public string StatusBadgeClass => Status switch
        {
            "Draft" => "bg-secondary",
            "PendingApproval" => "bg-warning text-dark",
            "L1Approved" => "bg-info text-dark",
            "Approved" => "bg-primary",
            "Paid" => "bg-success",
            "Rejected" => "bg-danger",
            _ => "bg-secondary"
        };

        public string StatusDisplay => Status switch
        {
            "PendingApproval" => "Pending Approval",
            "L1Approved" => "L1 Approved",
            _ => Status
        };
    }

    // ── Batch Line Item (1 per task in batch) ──────────────────────
    public class PayoutBatchLine
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskTypeName { get; set; } = string.Empty;
        public string TaskCategoryName { get; set; } = string.Empty;
        public string? ProjectModuleName { get; set; }
        public int TimeSpentMinutes { get; set; }
        public decimal RateApplied { get; set; }
        public string RateSource { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? TaskCompletedAt { get; set; }

        public string FormattedTime
        {
            get
            {
                if (TimeSpentMinutes == 0) return "0m";
                var h = TimeSpentMinutes / 60;
                var m = TimeSpentMinutes % 60;
                if (h == 0) return $"{m}m";
                if (m == 0) return $"{h}h";
                return $"{h}h {m}m";
            }
        }

        public string RateSourceBadgeClass => RateSource switch
        {
            "Project" => "bg-primary",
            "Category" => "bg-info text-dark",
            "TaskType" => "bg-warning text-dark",
            "Default" => "bg-secondary",
            "Hourly" => "bg-success",
            _ => "bg-secondary"
        };
    }

    // ── Approval History ───────────────────────────────────────────
    public class PayoutApprovalHistory
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int ApproverLevel { get; set; }   // 1 = Core Member, 2 = Admin/Finance
        public int ApprovedById { get; set; }
        public string Status { get; set; } = string.Empty;  // 'Approved' or 'Rejected'
        public string? Remarks { get; set; }
        public DateTime ApprovedAt { get; set; }

        // Display
        public string? ApprovedByName { get; set; }

        public string LevelDisplay => ApproverLevel switch
        {
            1 => "L1 — Core Member",
            2 => "L2 — Admin / Finance",
            _ => $"Level {ApproverLevel}"
        };
    }
}

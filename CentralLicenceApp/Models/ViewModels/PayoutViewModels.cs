using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Models.ViewModels
{
    // ── Configuration Index ────────────────────────────────────────
    public class PayoutConfigurationIndexViewModel
    {
        public List<PayoutConfigUserRow> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? ModelFilter { get; set; }    // "Hourly" / "Commission" / null
        public string? StatusFilter { get; set; }   // "Configured" / "NotConfigured" / null
    }

    public class PayoutConfigUserRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public bool IsConfigured { get; set; }
        public string? PayoutModel { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? DefaultCommissionAmount { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public int CommissionRuleCount { get; set; }
    }

    // ── Configuration Setup Form ───────────────────────────────────
    public class PayoutConfigurationFormViewModel
    {
        public int UserId { get; set; }
        [BindNever]
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Payout model is required.")]
        public string PayoutModel { get; set; } = "Hourly";

        [Range(0.01, 999999.99, ErrorMessage = "Hourly rate must be between 0.01 and 999,999.99.")]
        public decimal? HourlyRate { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Default commission must be between 0.01 and 999,999.99.")]
        public decimal? DefaultCommissionAmount { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsEdit { get; set; }
    }

    // ── Commission Rules Page ──────────────────────────────────────
    public class PayoutCommissionRulesViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public decimal? DefaultCommissionAmount { get; set; }

        public List<PayoutCommissionRule> Rules { get; set; } = new();
        public List<TaskTypeMaster> TaskTypes { get; set; } = new();
        public List<TaskCategoryMaster> TaskCategories { get; set; } = new();
        public List<ProjectModuleMaster> Projects { get; set; } = new();
    }

    // ── Commission Rule Form (AJAX) ────────────────────────────────
    public class PayoutCommissionRuleFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? TaskTypeId { get; set; }
        public int? TaskCategoryId { get; set; }
        public int? ProjectModuleId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    }

    // ── Batch Index ────────────────────────────────────────────────
    public class PayoutBatchIndexViewModel
    {
        public List<PayoutBatch> Batches { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public int? UserFilter { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Summary
        public int TotalBatches => Batches.Count;
        public decimal TotalGross => Batches.Sum(b => b.GrossAmount);
        public decimal TotalNet => Batches.Sum(b => b.NetAmount);
    }

    // ── Generate Payout Form ───────────────────────────────────────
    public class PayoutGenerateFormViewModel
    {
        [Required(ErrorMessage = "Please select a user.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "From date is required.")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required.")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        public List<PayoutConfigUserRow> ConfiguredUsers { get; set; } = new();
    }

    // ── Batch Details ──────────────────────────────────────────────
    public class PayoutBatchDetailsViewModel
    {
        public PayoutBatch Batch { get; set; } = new();
        public List<PayoutBatchLine> Lines { get; set; } = new();
        public List<PayoutApprovalHistory> ApprovalHistory { get; set; } = new();
        public bool CanSubmitForApproval { get; set; }
        public bool CanApproveL1 { get; set; }
        public bool CanApproveL2 { get; set; }
        public bool CanSettle { get; set; }
        public bool CanDelete { get; set; }
    }

    // ── Approval Inbox ─────────────────────────────────────────────
    public class PayoutApprovalInboxViewModel
    {
        public List<PayoutBatch> PendingBatches { get; set; } = new();
        public int ApproverLevel { get; set; }  // 1 or 2
        public string LevelLabel => ApproverLevel == 1 ? "L1 — Core Member Review" : "L2 — Admin / Finance Review";
    }

    // ── Settlement Form ────────────────────────────────────────────
    public class PayoutSettlementFormViewModel
    {
        [Required]
        public int BatchId { get; set; }

        public string? UserName { get; set; }
        public decimal NetAmount { get; set; }

        [Required(ErrorMessage = "Settlement amount is required.")]
        [Range(0.01, 99999999.99, ErrorMessage = "Settlement amount must be greater than zero.")]
        public decimal SettlementAmount { get; set; }

        [Required(ErrorMessage = "Settlement date is required.")]
        [DataType(DataType.Date)]
        public DateTime SettlementDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Payment mode is required.")]
        public string SettlementMode { get; set; } = string.Empty;

        public string? SettlementReferenceNo { get; set; }

        public int? SettlementBankId { get; set; }

        public string? SettlementRemarks { get; set; }

        // Dropdowns
        public List<BankMaster> Banks { get; set; } = new();
        public List<PaymentMode> PaymentModes { get; set; } = new();
    }

    // ── Preview (AJAX response) ────────────────────────────────────
    public class PayoutPreviewResult
    {
        public int EligibleTasks { get; set; }
        public int TotalMinutes { get; set; }
        public decimal EstimatedAmount { get; set; }
        public string PayoutModel { get; set; } = string.Empty;
        public string FormattedTime { get; set; } = string.Empty;
    }

    // ── Report: Payout Summary ─────────────────────────────────────
    public class PayoutSummaryReportViewModel
    {
        public List<PayoutSummaryRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalGross => Items.Sum(x => x.TotalGrossAmount);
        public decimal GrandTotalNet => Items.Sum(x => x.TotalNetAmount);
        public int GrandTotalTasks => Items.Sum(x => x.TotalTasks);
        public int GrandTotalMinutes => Items.Sum(x => x.TotalMinutes);
    }

    public class PayoutSummaryRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string PayoutModel { get; set; } = string.Empty;
        public int BatchCount { get; set; }
        public int TotalTasks { get; set; }
        public int TotalMinutes { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
    }

    // ── Report: Payout Detail ──────────────────────────────────────
    public class PayoutDetailReportViewModel
    {
        public List<PayoutDetailRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public int? BatchIdFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalAmount => Items.Sum(x => x.Amount);
        public int GrandTotalMinutes => Items.Sum(x => x.TimeSpentMinutes);
    }

    public class PayoutDetailRow
    {
        public int BatchId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string BatchPeriod { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
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
    }

    // ── Report: Payout History ─────────────────────────────────────
    public class PayoutHistoryReportViewModel
    {
        public List<PayoutHistoryRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public string? StatusFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalNet => Items.Sum(x => x.NetAmount);
        public decimal GrandTotalPaid => Items.Where(x => x.Status == "Paid").Sum(x => x.SettlementAmount ?? 0);
    }

    public class PayoutHistoryRow
    {
        public int BatchId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PayoutModel { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int TotalMinutes { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public string? GeneratedByName { get; set; }
        // Settlement
        public decimal? SettlementAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? SettledByName { get; set; }
        public string? SettlementMode { get; set; }
        public string? SettlementReferenceNo { get; set; }
        public string? BankName { get; set; }
    }
}

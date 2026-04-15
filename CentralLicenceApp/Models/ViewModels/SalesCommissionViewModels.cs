using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Models.ViewModels
{
    // ══════════════════════════════════════════════════════════════
    // CONFIGURATION VIEW MODELS
    // ══════════════════════════════════════════════════════════════

    public class SalesCommConfigIndexViewModel
    {
        public List<SalesCommConfigUserRow> Users { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? TypeFilter { get; set; }     // "Percentage" / "FixedAmount" / null
        public string? StatusFilter { get; set; }   // "Configured" / "NotConfigured" / null
    }

    public class SalesCommConfigUserRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
        public bool IsConfigured { get; set; }
        public string? CommissionType { get; set; }
        public decimal? DefaultRate { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public int RuleCount { get; set; }
    }

    public class SalesCommConfigFormViewModel
    {
        public int UserId { get; set; }
        [BindNever]
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }

        [Required(ErrorMessage = "Commission type is required.")]
        public string CommissionType { get; set; } = "Percentage";

        [Required(ErrorMessage = "Default rate is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Rate must be between 0.01 and 999,999.99.")]
        public decimal DefaultRate { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsEdit { get; set; }
    }

    public class SalesCommRulesViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string? CommissionType { get; set; }
        public decimal? DefaultRate { get; set; }

        public List<SalesCommissionRule> Rules { get; set; } = new();
        public List<ProductMaster> Products { get; set; } = new();
    }

    public class SalesCommRuleFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Commission type is required.")]
        public string CommissionType { get; set; } = "Percentage";

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Rate must be between 0.01 and 999,999.99.")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Effective date is required.")]
        [DataType(DataType.Date)]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;
    }

    // ══════════════════════════════════════════════════════════════
    // INVOICE ASSIGNMENT VIEW MODELS
    // ══════════════════════════════════════════════════════════════

    public class SalesInvoiceAssignmentIndexViewModel
    {
        public List<SalesInvoiceAssignmentRow> Assignments { get; set; } = new();
        public List<SalesInvoiceAssignmentRow> UnassignedInvoices { get; set; } = new();
        public List<UserMaster> SalesUsers { get; set; } = new();
        public List<ProductMaster> Products { get; set; } = new();
        public int? SalesUserFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Tab { get; set; }  // "assigned" or "unassigned"
    }

    public class SalesInvoiceAssignmentRow
    {
        public int? AssignmentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public string InvoiceStatus { get; set; } = string.Empty;
        public int? SalesUserId { get; set; }
        public string? SalesUserName { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public string? AssignedByName { get; set; }
        public bool IsAssigned => AssignmentId.HasValue;
    }

    // ══════════════════════════════════════════════════════════════
    // BATCH VIEW MODELS
    // ══════════════════════════════════════════════════════════════

    public class SalesCommBatchIndexViewModel
    {
        public List<SalesCommissionBatch> Batches { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public int? UserFilter { get; set; }
        public string? StatusFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public int TotalBatches => Batches.Count;
        public decimal TotalGross => Batches.Sum(b => b.GrossCommission);
        public decimal TotalNet => Batches.Sum(b => b.NetCommission);
    }

    public class SalesCommGenerateFormViewModel
    {
        [Required(ErrorMessage = "Please select a sales user.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "From date is required.")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "To date is required.")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        public List<SalesCommConfigUserRow> ConfiguredUsers { get; set; } = new();
    }

    public class SalesCommPreviewResult
    {
        public int EligiblePayments { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal EstimatedCommission { get; set; }
        public string CommissionType { get; set; } = string.Empty;
    }

    public class SalesCommBatchDetailsViewModel
    {
        public SalesCommissionBatch Batch { get; set; } = new();
        public List<SalesCommissionBatchLine> Lines { get; set; } = new();
        public List<SalesCommissionApprovalHistory> ApprovalHistory { get; set; } = new();
        public bool CanSubmitForApproval { get; set; }
        public bool CanApproveL1 { get; set; }
        public bool CanApproveL2 { get; set; }
        public bool CanSettle { get; set; }
        public bool CanDelete { get; set; }
    }

    public class SalesCommApprovalInboxViewModel
    {
        public List<SalesCommissionBatch> PendingBatches { get; set; } = new();
        public int ApproverLevel { get; set; }
        public string LevelLabel => ApproverLevel == 1 ? "L1 — Core Member Review" : "L2 — Admin / Finance Review";
    }

    public class SalesCommSettlementFormViewModel
    {
        [Required]
        public int BatchId { get; set; }

        public string? UserName { get; set; }
        public decimal NetCommission { get; set; }

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

        public List<BankMaster> Banks { get; set; } = new();
        public List<PaymentMode> PaymentModes { get; set; } = new();
    }

    // ══════════════════════════════════════════════════════════════
    // REPORT VIEW MODELS
    // ══════════════════════════════════════════════════════════════

    public class SalesCommSummaryReportViewModel
    {
        public List<SalesCommSummaryRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalSales => Items.Sum(x => x.TotalSalesAmount);
        public decimal GrandTotalGross => Items.Sum(x => x.TotalGrossCommission);
        public decimal GrandTotalNet => Items.Sum(x => x.TotalNetCommission);
    }

    public class SalesCommSummaryRow
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string CommissionType { get; set; } = string.Empty;
        public int BatchCount { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalGrossCommission { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetCommission { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalPending { get; set; }
    }

    public class SalesCommDetailReportViewModel
    {
        public List<SalesCommDetailRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public int? BatchIdFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalPayment => Items.Sum(x => x.PaymentAmount);
        public decimal GrandTotalCommission => Items.Sum(x => x.CommissionAmount);
    }

    public class SalesCommDetailRow
    {
        public int BatchId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string BatchPeriod { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string? ProductName { get; set; }
        public string CommissionType { get; set; } = string.Empty;
        public decimal RateApplied { get; set; }
        public string RateSource { get; set; } = string.Empty;
        public decimal CommissionAmount { get; set; }
    }

    public class SalesCommHistoryReportViewModel
    {
        public List<SalesCommHistoryRow> Items { get; set; } = new();
        public List<UserMaster> Users { get; set; } = new();
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? UserFilter { get; set; }
        public string? StatusFilter { get; set; }
        public bool IsAdminView { get; set; }

        public decimal GrandTotalNet => Items.Sum(x => x.NetCommission);
        public decimal GrandTotalPaid => Items.Where(x => x.Status == "Paid").Sum(x => x.SettlementAmount ?? 0);
    }

    public class SalesCommHistoryRow
    {
        public int BatchId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string CommissionType { get; set; } = string.Empty;
        public int TotalPayments { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal GrossCommission { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetCommission { get; set; }
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

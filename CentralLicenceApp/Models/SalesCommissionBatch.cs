using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    // ── Sales Commission Batch ─────────────────────────────────────
    public class SalesCommissionBatch
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        [Required]
        public DateTime ToDate { get; set; }

        public string CommissionTypeSnapshot { get; set; } = string.Empty;
        public decimal DefaultRateSnapshot { get; set; }
        public int TotalInvoices { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal GrossCommission { get; set; }
        public decimal DeductionAmount { get; set; }
        public decimal NetCommission { get; set; }

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

        public string CommissionTypeDisplay =>
            CommissionTypeSnapshot == "Percentage" ? "Percentage (%)" : "Fixed Amount (₹)";
    }

    // ── Batch Line Item ────────────────────────────────────────────
    public class SalesCommissionBatchLine
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int InvoicePaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string CommissionType { get; set; } = string.Empty;
        public decimal RateApplied { get; set; }
        public string RateSource { get; set; } = string.Empty;
        public decimal CommissionAmount { get; set; }

        public string RateDisplay =>
            CommissionType == "Percentage" ? $"{RateApplied}%" : $"₹{RateApplied:N2}";

        public string RateSourceBadgeClass => RateSource switch
        {
            "Product" => "bg-primary",
            "Default" => "bg-secondary",
            _ => "bg-secondary"
        };
    }

    // ── Approval History ───────────────────────────────────────────
    public class SalesCommissionApprovalHistory
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int ApproverLevel { get; set; }
        public int ApprovedById { get; set; }
        public string Status { get; set; } = string.Empty;
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

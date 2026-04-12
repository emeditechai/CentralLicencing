using System;

namespace CentralLicenceApp.Models
{
    public class ExpenseRequest
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public int? ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public string PurposeOfTravel { get; set; } = string.Empty;
        public string? EmployeeRemarks { get; set; }
        public string Status { get; set; } = ExpenseRequestStatus.Draft;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? ApprovalRemarks { get; set; }
        public int? ApprovedById { get; set; }
        public DateTime? ReimbursementStartedAt { get; set; }
        public int? ReimbursementStartedById { get; set; }
        public string? ReimbursementStartedByName { get; set; }
        public string? ReimbursementRemarks { get; set; }
        public decimal? SettlementAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public int? SettledById { get; set; }
        public string? SettledByName { get; set; }
        public string? SettlementMode { get; set; }
        public string? SettlementReferenceNo { get; set; }
        public string? SettlementRemarks { get; set; }
        public string? SettlementReceiptNumber { get; set; }

        public bool SettlementNotRequired { get; set; }

        public int? FinancialYearId { get; set; }
        public string? FYCode { get; set; }
    }
}
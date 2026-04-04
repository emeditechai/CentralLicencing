using System;

namespace CentralLicenceApp.Models.Reports
{
    public class ExpenseReportRow
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestedByUser { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string PurposeOfTravel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUser { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? SettledByUser { get; set; }
        public decimal? SettlementAmount { get; set; }
        public int TotalCount { get; set; }
    }
}

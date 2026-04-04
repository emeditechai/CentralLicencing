using System;

namespace CentralLicenceApp.Models.Reports
{
    public class SettlementReportRow
    {
        public int Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string RequestedByUser { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; }
        public string PurposeOfTravel { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal? SettlementAmount { get; set; }
        public DateTime? SettlementDate { get; set; }
        public DateTime? SettledAt { get; set; }
        public string? SettlementMode { get; set; }
        public string? SettlementReferenceNo { get; set; }
        public string? SettlementRemarks { get; set; }
        public string? SettlementReceiptNumber { get; set; }
        public string? SettledByUser { get; set; }
        public int TotalCount { get; set; }
    }
}

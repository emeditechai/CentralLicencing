using System;

namespace CentralLicenceApp.Models.Reports
{
    public class ClientDueRow
    {
        public int Id { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? PartyGSTINNo { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int OverdueDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? FYCode { get; set; }
        public int TotalCount { get; set; }
    }
}

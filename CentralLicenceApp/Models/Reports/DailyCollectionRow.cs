using System;

namespace CentralLicenceApp.Models.Reports
{
    public class DailyCollectionRow
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string PaymentModes { get; set; } = string.Empty;
        public decimal TotalAmountPaid { get; set; }
        public string CollectedBy { get; set; } = string.Empty;
        public string? FYCode { get; set; }
        public int TotalCount { get; set; }
    }
}

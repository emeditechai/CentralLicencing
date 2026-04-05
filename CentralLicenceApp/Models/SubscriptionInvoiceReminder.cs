using System;

namespace CentralLicenceApp.Models
{
    public class SubscriptionInvoiceReminder
    {
        public int PurchasedProductId { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        /// <summary>Monthly | Quarterly | Half Yearly | Annual</summary>
        public string BillingFrequency { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime NextRenewalDate { get; set; }
        public int DaysUntilRenewal { get; set; }
    }
}

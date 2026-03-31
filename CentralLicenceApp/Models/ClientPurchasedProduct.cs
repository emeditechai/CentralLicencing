using System;

namespace CentralLicenceApp.Models
{
    public class ClientPurchasedProduct
    {
        public int Id { get; set; }
        public int ClientDetailsId { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int ProductRateId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string PricingModel { get; set; } = string.Empty;
        public string BillingModel { get; set; } = string.Empty;
        public string BillingFrequency { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string AmcCalculationType { get; set; } = string.Empty;
        public decimal AmcPercentage { get; set; }
        public decimal AmcAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public bool IsSubscription => string.Equals(BillingModel, "Subscription", StringComparison.OrdinalIgnoreCase);

        public string BillingSummary => IsSubscription && !string.IsNullOrWhiteSpace(BillingFrequency)
            ? $"{BillingModel} • {BillingFrequency}"
            : BillingModel;
    }
}
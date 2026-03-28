using System;

namespace CentralLicenceApp.Models
{
    public class ProductRate
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string PricingModel { get; set; } = string.Empty;
        public string? ProductSpecification { get; set; }
        public string? Features { get; set; }
        public decimal Rate { get; set; }
        public string AmcCalculationType { get; set; } = string.Empty;
        public decimal AmcPercentage { get; set; }
        public decimal AmcAmount { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int DiscountOfferCount { get; set; }
        public int ActiveDiscountOfferCount { get; set; }
    }
}
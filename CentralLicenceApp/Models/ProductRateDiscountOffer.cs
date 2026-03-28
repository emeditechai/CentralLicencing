using System;

namespace CentralLicenceApp.Models
{
    public class ProductRateDiscountOffer
    {
        public int Id { get; set; }
        public int ProductRateId { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public string PricingModel { get; set; } = string.Empty;
        public decimal BaseRate { get; set; }
        public string DiscountName { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public string? PromoCode { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
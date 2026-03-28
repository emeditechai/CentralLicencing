using System;

namespace CentralLicenceApp.Models
{
    public class ProductMaster
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int PricingModelCount { get; set; }
        public int ActivePricingModelCount { get; set; }
        public decimal? MinRate { get; set; }
        public decimal? MaxRate { get; set; }
        public int DiscountOfferCount { get; set; }
        public int ActiveDiscountOfferCount { get; set; }
        public int ActivePromoCodeCount { get; set; }
        public int ExpiringSoonDiscountOfferCount { get; set; }
        public DateTime? NextDiscountExpiryDate { get; set; }
    }
}
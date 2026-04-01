using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ProductMasterFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Code is required")]
        [Display(Name = "Product Code")]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Name is required")]
        [Display(Name = "Product Name")]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Product Type is required")]
        [Display(Name = "Product Type")]
        [StringLength(50)]
        public string ProductType { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ProductRateFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        [Range(1, int.MaxValue, ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Pricing Model is required")]
        [Display(Name = "Pricing Model")]
        [StringLength(50)]
        public string PricingModel { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [Display(Name = "Model")]
        [StringLength(20)]
        public string BillingModel { get; set; } = string.Empty;

        [Display(Name = "Frequency")]
        [StringLength(20)]
        public string BillingFrequency { get; set; } = string.Empty;

        [Display(Name = "Product Specification")]
        [StringLength(500)]
        public string? ProductSpecification { get; set; }

        [Display(Name = "Features")]
        [StringLength(2000)]
        public string? Features { get; set; }

        [Required(ErrorMessage = "Rate is required")]
        [Display(Name = "Rate")]
        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Rate must be greater than zero")]
        public decimal Rate { get; set; }

        [Display(Name = "AMC Type")]
        [StringLength(20)]
        public string? AmcCalculationType { get; set; }

        [Display(Name = "AMC Value")]
        public decimal? AmcInputValue { get; set; }

        [Display(Name = "AMC Percentage")]
        public decimal CalculatedAmcPercentage { get; set; }

        [Display(Name = "AMC Amount")]
        public decimal CalculatedAmcAmount { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ProductRateIndexViewModel
    {
        public List<ProductMaster> Products { get; set; } = new();
        public List<ProductRate> Items { get; set; } = new();
        public int? SelectedProductId { get; set; }
        public ProductMaster? SelectedProduct { get; set; }
    }

    public class ProductMasterDetailsViewModel
    {
        public ProductMaster Product { get; set; } = new();
        public List<ProductRate> ProductRates { get; set; } = new();
    }

    public class ProductRateDiscountFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pricing Model is required")]
        [Display(Name = "Pricing Model")]
        [Range(1, int.MaxValue, ErrorMessage = "Pricing Model is required")]
        public int ProductRateId { get; set; }

        [Required(ErrorMessage = "Discount Name is required")]
        [Display(Name = "Discount Name")]
        [StringLength(100)]
        public string DiscountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount Model is required")]
        [Display(Name = "Discount Model")]
        [StringLength(30)]
        public string DiscountType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount Value is required")]
        [Display(Name = "Discount Value")]
        [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Discount value must be greater than zero")]
        public decimal DiscountValue { get; set; }

        [Display(Name = "Promo Code")]
        [StringLength(50)]
        public string? PromoCode { get; set; }

        [Required(ErrorMessage = "Validity start date is required")]
        [Display(Name = "Valid From")]
        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [Required(ErrorMessage = "Validity end date is required")]
        [Display(Name = "Valid To")]
        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ProductRateDiscountIndexViewModel
    {
        public List<ProductRate> ProductRates { get; set; } = new();
        public List<ProductRateDiscountOffer> Items { get; set; } = new();
        public int? SelectedProductRateId { get; set; }
        public ProductRate? SelectedProductRate { get; set; }
        public bool TodayOnly { get; set; }
    }
}
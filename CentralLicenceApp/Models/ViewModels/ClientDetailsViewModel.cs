using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ClientPurchasedProductEntryViewModel
    {
        [Display(Name = "Product")]
        [Range(1, int.MaxValue, ErrorMessage = "Product is required")]
        public int? ProductId { get; set; }

        [Display(Name = "Pricing Model")]
        [Range(1, int.MaxValue, ErrorMessage = "Pricing Model is required")]
        public int? ProductRateId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string PricingModel { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string AmcCalculationType { get; set; } = string.Empty;
        public decimal AmcPercentage { get; set; }
        public decimal AmcAmount { get; set; }
    }

    public class ClientDetailsViewModel
    {
        public int ID { get; set; }

        public string ClientCode { get; set; } = string.Empty;
        public string? ClientName { get; set; }   // read-only display from ClientAppLicense

        [Display(Name = "Contact Person Name")]
        public string? ClientPersonName { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Products Purchased")]
        public List<ClientPurchasedProductEntryViewModel> PurchasedProducts { get; set; } = new();

        [Display(Name = "Is Internal Use")]
        public bool IsInternalUse { get; set; }

        [Display(Name = "Reference ClientCode")]
        [StringLength(20)]
        public string? ReferenceClientCode { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [Display(Name = "Anniversary Date")]
        [DataType(DataType.Date)]
        public DateTime? Anniversarydate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public bool IsNew => ID == 0;
    }
}

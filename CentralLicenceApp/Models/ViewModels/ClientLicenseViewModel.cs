using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class ClientLicenseFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Client Code")]
        public string? ClientCode { get; set; }

        [Required(ErrorMessage = "Product Type is required")]
        [Display(Name = "Product Type")]
        public string ProductType { get; set; } = "eRestoPOS";

        public static readonly List<string> AvailableProductTypes = new()
        {
            "eRestoPOS",
            "eLUXstay",
            "e360Pluscare"
        };

        [Required(ErrorMessage = "Client Name is required")]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Number is required")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; } = string.Empty;

        [Display(Name = "Email ID")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string? EmailID { get; set; }

        [Display(Name = "Application URL")]
        public string? AppUrl { get; set; }

        [Required(ErrorMessage = "Expiry Date is required")]
        [Display(Name = "Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Display(Name = "AMC Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime? AMC_Expireddate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    public class ClientLicenseListViewModel
    {
        public List<ClientAppLicense> Licenses { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? ProductType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public HashSet<string> ClientCodesWithDetails { get; set; } = new();
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
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
        public List<string> SelectedProducts { get; set; } = new();

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [Display(Name = "Anniversary Date")]
        [DataType(DataType.Date)]
        public DateTime? Anniversarydate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // For the multi-select dropdown
        public static readonly List<string> AvailableProducts = new()
        {
            "eRestoPOS",
            "eLUXstay",
            "e360Pluscare"
        };

        public bool IsNew => ID == 0;
    }
}

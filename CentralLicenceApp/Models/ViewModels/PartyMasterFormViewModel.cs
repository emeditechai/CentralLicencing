using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class PartyMasterFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Party Name is required")]
        [StringLength(150)]
        [Display(Name = "Party Name")]
        public string PartyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Display(Name = "Mobile")]
        public string? Mobile { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(300)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(20)]
        [Display(Name = "GSTIN No")]
        public string? GSTINNo { get; set; }

        [StringLength(10)]
        [Display(Name = "PAN No")]
        public string? PANNo { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class QuotationLineViewModel
    {
        public int SNo { get; set; }

        [Required(ErrorMessage = "Item description is required")]
        [Display(Name = "Item / Description")]
        public string ItemDescription { get; set; } = string.Empty;

        [Display(Name = "Plan")]
        public string? PlanName { get; set; }

        [Display(Name = "Type")]
        public string? Type { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Qty must be at least 1")]
        [Display(Name = "Qty")]
        public int Qty { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Rate must be 0 or more")]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Range(0, 100, ErrorMessage = "Discount % must be between 0 and 100")]
        [Display(Name = "Disc %")]
        public decimal DiscountPercent { get; set; }

        // Computed server-side
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }

        [Range(0, 100)]
        [Display(Name = "GST %")]
        public decimal GstPercent { get; set; }

        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
    }

    public class QuotationFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Quotation No")]
        public string QuotationNo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Quotation Date")]
        public DateTime QuotationDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Valid Until")]
        public DateTime? ValidUntilDate { get; set; }

        [Required(ErrorMessage = "Please select a party")]
        [Display(Name = "Party")]
        public int PartyId { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Terms & Conditions")]
        public string? TermsAndConditions { get; set; }

        public List<QuotationLineViewModel> Lines { get; set; } = new();

        /// <summary>Selected authorised signatory user IDs (max 3).</summary>
        public List<int> SignatoryUserIds { get; set; } = new();

        public bool IsNew => Id == 0;
    }
}

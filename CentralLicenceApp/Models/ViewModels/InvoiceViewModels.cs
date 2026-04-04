using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class InvoiceLineViewModel
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

        [Range(0, double.MaxValue)]
        [Display(Name = "Rate")]
        public decimal Rate { get; set; }

        [Range(0, 100)]
        [Display(Name = "Disc %")]
        public decimal DiscountPercent { get; set; }

        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }

        [Range(0, 100)]
        [Display(Name = "GST %")]
        public decimal GstPercent { get; set; }

        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
    }

    public class InvoiceFormViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice No")]
        public string InvoiceNo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        public int? QuotationId { get; set; }
        public string? QuotationNo { get; set; }

        [Required(ErrorMessage = "Please select a party")]
        [Display(Name = "Party")]
        public int PartyId { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Terms & Conditions")]
        public string? TermsAndConditions { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Received Amount")]
        public decimal ReceivedAmount { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Previous Balance")]
        public decimal PreviousBalance { get; set; }

        public List<InvoiceLineViewModel> Lines { get; set; } = new();

        public List<int> SignatoryUserIds { get; set; } = new();

        public bool IsNew => Id == 0;

        /// <summary>
        /// Advance payment lines captured via the payment modal on invoice creation.
        /// Only populated when ReceivedAmount > 0 during Create/Convert flow.
        /// </summary>
        public List<PaymentLineViewModel> AdvancePaymentLines { get; set; } = new();
    }
}

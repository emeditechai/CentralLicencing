using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class PaymentLineViewModel
    {
        public int PaymentModeId { get; set; }
        public string PaymentModeName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        public string? ReferenceNo { get; set; }

        // Card-specific
        public string? CardType     { get; set; }  // "Debit" | "Credit"
        public string? CardLastFour { get; set; }  // 4 numeric digits

        // Bank-specific (Cards, Cheque, NEFT, RTGS, IMPS, Bank Transfer)
        public int?    BankId       { get; set; }
    }

    public class PaymentFormViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;

        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        public string? Notes { get; set; }

        public List<PaymentLineViewModel> Lines { get; set; } = new();

        // Display-only — populated on GET, not re-validated on POST
        public decimal InvoiceTotalAmount { get; set; }
        public decimal AlreadyPaid { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal OutstandingBalance { get; set; }
    }
}

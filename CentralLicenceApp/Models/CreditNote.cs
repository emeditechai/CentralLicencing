using System;

namespace CentralLicenceApp.Models
{
    public class CreditNote
    {
        public int Id { get; set; }
        public string CreditNoteNo { get; set; } = string.Empty;

        public int RefundId { get; set; }
        public string RefundNo { get; set; } = string.Empty;

        public int PaymentId { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;

        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;

        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? PartyAddress { get; set; }
        public string? PartyGSTINNo { get; set; }
        public string? PartyPANNo { get; set; }
        public string? PartyContactPerson { get; set; }
        public string? PartyMobile { get; set; }

        public DateTime CreditNoteDate { get; set; }
        public decimal Amount { get; set; }

        public int PaymentModeId { get; set; }
        public string PaymentModeName { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public string? Reason { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

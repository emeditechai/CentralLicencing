using System;
using System.Collections.Generic;

namespace CentralLicenceApp.Models
{
    public class InvoicePayment
    {
        public int Id { get; set; }
        public string ReceiptNo { get; set; } = string.Empty;

        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;

        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;

        public DateTime PaymentDate { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string? Notes { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Void / cancel tracking
        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }
        public string? VoidedBy { get; set; }
        public string? VoidRemarks { get; set; }

        /// <summary>Populated from JOIN with Invoice.Status</summary>
        public string InvoiceStatus { get; set; } = string.Empty;

        public List<InvoicePaymentLine> Lines { get; set; } = new();

        /// <summary>Populated by repository — refunds issued against this payment.</summary>
        public List<InvoiceRefund> Refunds { get; set; } = new();

        /// <summary>Net effective amount = TotalAmountPaid minus refunds.</summary>
        public decimal NetAmountPaid => TotalAmountPaid - Refunds.Sum(r => r.Amount);
    }

    public class InvoiceRefund
    {
        public int Id { get; set; }
        public string RefundNo { get; set; } = string.Empty;
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public DateTime RefundDate { get; set; }
        public decimal Amount { get; set; }
        public int PaymentModeId { get; set; }
        public string PaymentModeName { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>Populated via LEFT JOIN with CreditNote table — null if no credit note issued yet.</summary>
        public int? CreditNoteId { get; set; }
        public string? CreditNoteNo { get; set; }
    }

    public class InvoicePaymentLine
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int PaymentModeId { get; set; }
        public string PaymentModeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ReferenceNo { get; set; }
        // Card-specific fields
        public string? CardType     { get; set; }
        public string? CardLastFour { get; set; }
        // Bank-specific fields
        public int?    BankId       { get; set; }
        public string? BankName     { get; set; }
    }
}

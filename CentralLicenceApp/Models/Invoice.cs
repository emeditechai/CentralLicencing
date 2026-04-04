using System;
using System.Collections.Generic;

namespace CentralLicenceApp.Models
{

    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }

        public int? QuotationId { get; set; }
        public string? QuotationNo { get; set; }

        // Party snapshot
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? PartyAddress { get; set; }
        public string? PartyGSTINNo { get; set; }
        public string? PartyPANNo { get; set; }
        public string? PartyContactPerson { get; set; }
        public string? PartyMobile { get; set; }

        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TotalCgst { get; set; }
        public decimal TotalSgst { get; set; }
        public decimal TotalIgst { get; set; }
        public decimal TotalGst => TotalCgst + TotalSgst + TotalIgst;
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ReceivedAmount { get; set; }
        public decimal PreviousBalance { get; set; }

        /// <summary>CurrentBalance = TotalAmount - ReceivedAmount (this invoice's own outstanding only)</summary>
        public decimal CurrentBalance => TotalAmount - ReceivedAmount;

        /// <summary>Draft | Sent | Paid | Partial | Cancelled</summary>
        public string Status { get; set; } = "Draft";

        public string? CancelRemarks { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<InvoiceLine> Lines { get; set; } = new();

        public bool IsCancelled => Status == "Cancelled";

        public List<int> SignatoryUserIds { get; set; } = new();
        public List<UserMaster> Signatories { get; set; } = new();
    }

    public class InvoiceLine
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public int SNo { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public string? PlanName { get; set; }
        public string? Type { get; set; }
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }

        // GST
        public decimal GstPercent { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal IgstAmount { get; set; }
    }
}

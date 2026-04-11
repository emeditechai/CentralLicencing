using System;
using System.Collections.Generic;

namespace CentralLicenceApp.Models
{
    public class Quotation
    {
        public int Id { get; set; }
        public string QuotationNo { get; set; } = string.Empty;
        public DateTime QuotationDate { get; set; }
        public DateTime? ValidUntilDate { get; set; }

        // Party snapshot (stored at time of creation)
        public int PartyId { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? PartyAddress { get; set; }
        public string? PartyGSTINNo { get; set; }
        public string? PartyPANNo { get; set; }
        public string? PartyContactPerson { get; set; }
        public string? PartyMobile { get; set; }

        public string? Notes { get; set; }
        /// <summary>FK to TermsConditionTemplate master. Populated from JOIN on load.</summary>
        public int? TermsConditionTemplateId { get; set; }
        /// <summary>Populated from TermsConditionTemplate.Description via JOIN — not stored directly.</summary>
        public string? TermsAndConditions { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TotalCgst { get; set; }
        public decimal TotalSgst { get; set; }
        public decimal TotalIgst { get; set; }
        public decimal TotalGst => TotalCgst + TotalSgst + TotalIgst;
        public bool EnableRoundOff { get; set; }
        public decimal RoundOff { get; set; }
        public decimal TotalAmount { get; set; }

        /// <summary>Draft | Sent | Accepted | Rejected | Converted | Cancelled</summary>
        public string Status { get; set; } = "Draft";

        public string? CancelRemarks { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? FinancialYearId { get; set; }
        public string? FYCode { get; set; }

        public List<QuotationLine> Lines { get; set; } = new();

        /// <summary>Up to 3 selected authorised signatories (user IDs).</summary>
        public List<int> SignatoryUserIds { get; set; } = new();

        /// <summary>Populated on load for Print view.</summary>
        public List<UserMaster> Signatories { get; set; } = new();

        public bool IsConverted  => Status == "Converted";
        public bool IsCancelled  => Status == "Cancelled";
    }

    public class QuotationLine
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
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

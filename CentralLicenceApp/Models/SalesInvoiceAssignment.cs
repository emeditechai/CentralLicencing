using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class SalesInvoiceAssignment
    {
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int SalesUserId { get; set; }

        public int? ProductId { get; set; }

        public int AssignedById { get; set; }
        public DateTime AssignedAt { get; set; }

        // Display
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? PartyName { get; set; }
        public decimal? InvoiceTotalAmount { get; set; }
        public string? InvoiceStatus { get; set; }
        public string? SalesUserName { get; set; }
        public string? ProductName { get; set; }
        public string? AssignedByName { get; set; }
        public decimal? ReceivedAmount { get; set; }

        // Line-level assignment details
        public List<SalesInvoiceAssignmentLine> Lines { get; set; } = new();
    }

    public class SalesInvoiceAssignmentLine
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int InvoiceLineId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }

        [Required]
        [RegularExpression("^(Percentage|FixedAmount)$")]
        public string CommissionType { get; set; } = "Percentage";

        [Required]
        [Range(0.01, 999999.99)]
        public decimal CommissionRate { get; set; }

        public decimal CommissionAmount { get; set; }
    }
}

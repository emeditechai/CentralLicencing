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
    }
}

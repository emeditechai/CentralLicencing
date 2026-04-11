namespace CentralLicenceApp.Models
{
    public class TicketMessage
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int SenderId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display helpers
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfileImage { get; set; }

        // Attachments loaded separately
        public List<TicketAttachment> Attachments { get; set; } = new();
    }
}

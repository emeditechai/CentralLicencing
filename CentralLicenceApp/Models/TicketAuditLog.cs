namespace CentralLicenceApp.Models
{
    public class TicketAuditLog
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int PerformedById { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display helpers
        public string PerformedByName { get; set; } = string.Empty;
    }
}

using System;

namespace CentralLicenceApp.Models
{
    public class EmailLogEntry
    {
        public int Id { get; set; }
        public string EmailType { get; set; } = string.Empty;
        public string? TemplateKey { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientName { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
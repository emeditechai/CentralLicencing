using System;

namespace CentralLicenceApp.Models
{
    public class ClientLicenseAuditLog
    {
        public int Id { get; set; }
        public int ClientLicenseId { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        /// <summary>"ExpiryDate" or "AMCExpiryDate"</summary>
        public string FieldChanged { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class MailConfiguration
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string SmtpServer { get; set; } = string.Empty;

        [Required]
        public int SmtpPort { get; set; }

        [Required, MaxLength(200)]
        public string SmtpUsername { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string SmtpPassword { get; set; } = string.Empty;

        public bool EnableSSL { get; set; } = true;

        [Required, EmailAddress, MaxLength(200)]
        public string FromEmail { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string FromName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? AdminNotificationEmail { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

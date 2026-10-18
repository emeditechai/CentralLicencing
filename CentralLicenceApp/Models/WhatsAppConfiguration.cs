using System;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class WhatsAppConfiguration
    {
        public int Id { get; set; }

        [Required, MaxLength(100), Display(Name = "Configuration Name")]
        public string ConfigName { get; set; } = "Primary WaSender API";

        [Required, MaxLength(500), Url, Display(Name = "API Endpoint URL")]
        public string ApiUrl { get; set; } = "https://wasenderapi.com/api/send-message";

        [MaxLength(50), Display(Name = "Auth Header Name")]
        public string? AuthHeaderKey { get; set; } = "Authorization";

        // Data Protection ciphertext; decrypted only inside WhatsAppService.
        public string? ApiKeyEncrypted { get; set; }
        public string? ApiKeyLast4 { get; set; }

        [MaxLength(10), Display(Name = "Default Country Code")]
        public string? DefaultCountryCode { get; set; } = "+91";

        [MaxLength(50), Display(Name = "Sender Phone")]
        public string? SenderPhone { get; set; }

        [Display(Name = "Master WhatsApp Service")]
        public bool IsEnabled { get; set; } = true;

        [Display(Name = "Send Email Notifications on WhatsApp")]
        public bool NotifyOnEmail { get; set; } = true;

        [Range(1000, 60000), Display(Name = "Minimum Gap Between Sends (ms)")]
        public int MinGapMs { get; set; } = 5500;

        public DateTime? LastTestedDate { get; set; }
        public string? LastTestResult { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool HasApiKey => !string.IsNullOrEmpty(ApiKeyEncrypted);
    }

    public class WhatsAppLog
    {
        public int Id { get; set; }
        public int? ConfigId { get; set; }
        public string ModuleCode { get; set; } = string.Empty;
        public int? ModuleRefId { get; set; }
        public string RecipientPhone { get; set; } = string.Empty;
        public string? MessageText { get; set; }
        public bool IsSuccess { get; set; }
        public string? MsgId { get; set; }
        public string? Jid { get; set; }
        public string? Status { get; set; }
        public string? ApiResponse { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SentBy { get; set; }
        public DateTime SentDate { get; set; }
    }

    public class WhatsAppSendResult
    {
        public bool Success { get; set; }
        public string? MsgId { get; set; }
        public string? Jid { get; set; }
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
        public string? RawResponse { get; set; }
        public string? RecipientPhone { get; set; }
    }
}

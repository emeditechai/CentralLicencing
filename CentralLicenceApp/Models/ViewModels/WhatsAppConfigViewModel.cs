using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class WhatsAppConfigViewModel
    {
        public WhatsAppConfiguration Config { get; set; } = new();

        // Write-only: blank keeps the stored token, a value replaces it.
        [MaxLength(500), Display(Name = "API Token (Bearer Key)")]
        public string? NewApiKey { get; set; }

        public IEnumerable<WhatsAppLog> RecentLogs { get; set; } = new List<WhatsAppLog>();
    }

    public class WhatsAppTestRequest
    {
        public string? ToPhone { get; set; }
        public string? MessageText { get; set; }
        public string? ApiKey { get; set; }   // optional: unsaved token typed on the screen
    }
}

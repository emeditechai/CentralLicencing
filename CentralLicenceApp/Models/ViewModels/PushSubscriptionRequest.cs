using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models.ViewModels
{
    public class PushSubscriptionRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Endpoint { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string P256dh { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Auth { get; set; } = string.Empty;
    }
}
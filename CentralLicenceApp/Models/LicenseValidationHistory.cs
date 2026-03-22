using System;

namespace CentralLicenceApp.Models
{
    public class LicenseValidationHistory
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public string? FailureReason { get; set; }
        public string? PublicIPAddress { get; set; }
        public string? DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AppUrl { get; set; }
    }
}

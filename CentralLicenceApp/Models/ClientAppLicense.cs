using System;

namespace CentralLicenceApp.Models
{
    public class ClientAppLicense
    {
        public int Id { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string LicenseKey { get; set; } = string.Empty;
        public string HardDiskNumber { get; set; } = string.Empty;
        public string ServerMacID { get; set; } = string.Empty;
        public string MotherboardNumber { get; set; } = string.Empty;
        public DateTime Startdate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool OTP_Verified { get; set; }
        public string? PublicIPAddress { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string? EmailID { get; set; }
        public DateTime? AMC_Expireddate { get; set; }
        public string? AppUrl { get; set; }
        public string? ConnectionString { get; set; }
        public string ProductType { get; set; } = "eRestoPOS";
    }
}

using System;

namespace CentralLicenceApp.Models
{
    public class ClientDetails
    {
        public int ID { get; set; }
        public string ClientCode { get; set; } = string.Empty;
        public string? ClientPersonName { get; set; }
        public string? Address { get; set; }
        public string? ProductPurchased { get; set; }   // comma-separated
        public DateTime? DOB { get; set; }
        public DateTime? Anniversarydate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

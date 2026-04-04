using System;

namespace CentralLicenceApp.Models
{
    public class PartyMaster
    {
        public int Id { get; set; }
        public string PartyName { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GSTINNo { get; set; }
        public string? PANNo { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

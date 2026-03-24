using System;

namespace CentralLicenceApp.Models
{
    public class LocationMaster
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

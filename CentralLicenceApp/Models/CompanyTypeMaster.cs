using System;

namespace CentralLicenceApp.Models
{
    public class CompanyTypeMaster
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
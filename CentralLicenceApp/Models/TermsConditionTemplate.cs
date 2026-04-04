using System;

namespace CentralLicenceApp.Models
{
    public class TermsConditionTemplate
    {
        public int Id { get; set; }
        public string TermsName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

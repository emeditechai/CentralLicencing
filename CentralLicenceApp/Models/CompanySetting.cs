using System;

namespace CentralLicenceApp.Models
{
    public class CompanySetting
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public int CompanyTypeId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? District { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Website { get; set; }
        public string? EmailId { get; set; }
        public string? ContactNo { get; set; }
        public string? Pincode { get; set; }
        public string? GSTCode { get; set; }
        public string? PANCard { get; set; }
        public int? ParentCompanyId { get; set; }
        public bool IsParentCompany { get; set; }
        public bool IsExpenseEmailNotificationRequired { get; set; }
        public string? CompanyLogoPath { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public string? CompanyTypeName { get; set; }
        public string? ParentCompanyName { get; set; }
        public string? ParentCompanyCity { get; set; }
        public string ParentCompanyDisplayName => string.IsNullOrWhiteSpace(ParentCompanyName)
            ? string.Empty
            : string.IsNullOrWhiteSpace(ParentCompanyCity)
                ? ParentCompanyName
                : $"{ParentCompanyName} - {ParentCompanyCity}";
    }
}
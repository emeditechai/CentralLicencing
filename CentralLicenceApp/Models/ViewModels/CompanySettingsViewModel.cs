using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CentralLicenceApp.Models;
using Microsoft.AspNetCore.Http;

namespace CentralLicenceApp.Models.ViewModels
{
    public class CompanySettingsFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Company Code is required")]
        [Display(Name = "Company Code")]
        [StringLength(50)]
        public string CompanyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company Type is required")]
        [Display(Name = "Company Type")]
        public int CompanyTypeId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Company Name")]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [Display(Name = "Website")]
        [StringLength(200)]
        [Url(ErrorMessage = "Enter a valid website URL")]
        public string? Website { get; set; }

        [Display(Name = "Email ID")]
        [StringLength(200)]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string? EmailId { get; set; }

        [Display(Name = "Contact No")]
        [StringLength(30)]
        [Phone(ErrorMessage = "Enter a valid contact number")]
        public string? ContactNo { get; set; }

        [Display(Name = "Pincode")]
        [StringLength(20)]
        public string? Pincode { get; set; }

        [Display(Name = "GST Code")]
        [StringLength(50)]
        public string? GSTCode { get; set; }

        [Display(Name = "PAN Card")]
        [StringLength(50)]
        public string? PANCard { get; set; }

        [Display(Name = "Parent Company")]
        public int? ParentCompanyId { get; set; }

        [Display(Name = "Is Parent Company")]
        public bool IsParentCompany { get; set; }

        [Display(Name = "Email Notification Required on Expense Request")]
        public bool IsExpenseEmailNotificationRequired { get; set; }

        [Display(Name = "Company Logo")]
        public IFormFile? CompanyLogo { get; set; }

        public string? ExistingLogoPath { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public List<CompanyTypeMaster> CompanyTypes { get; set; } = new();
        public List<CompanySetting> ParentCompanies { get; set; } = new();
    }
}
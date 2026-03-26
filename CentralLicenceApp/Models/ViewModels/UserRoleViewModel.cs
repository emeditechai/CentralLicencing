using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CentralLicenceApp.Models.ViewModels
{
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be 3-100 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Location")]
        public int? LocationId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Designation")]
        public int? DesignationId { get; set; }

        [Display(Name = "Employee Type")]
        public int? EmployeeTypeId { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Is Employee")]
        public bool IsEmployee { get; set; }

        [Display(Name = "Employee Code")]
        [StringLength(50)]
        public string? EmployeeCode { get; set; }

        [Display(Name = "Is Core Member")]
        public bool IsCoreMember { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingProfileImagePath { get; set; }

        public List<RoleMaster> Roles { get; set; } = new();
        public List<LocationMaster> Locations { get; set; } = new();
        public List<UserMaster> Managers { get; set; } = new();
        public List<EmployeeDepartmentMaster> Departments { get; set; } = new();
        public List<EmployeeDesignationMaster> Designations { get; set; } = new();
        public List<EmployeeTypeMaster> EmployeeTypes { get; set; } = new();
    }

    public class RoleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        [Display(Name = "Role Name")]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(200)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }
}


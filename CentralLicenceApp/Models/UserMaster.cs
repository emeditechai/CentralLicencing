using System;
using System.Collections.Generic;

namespace CentralLicenceApp.Models
{
    public class UserMaster
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<int> AssignedRoleIds { get; set; } = new();
        public List<RoleMaster> Roles { get; set; } = new();
        public string? RoleNamesDisplay { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int? DesignationId { get; set; }
        public string? DesignationName { get; set; }
        public int? EmployeeTypeId { get; set; }
        public string? EmployeeTypeName { get; set; }
        public bool IsEmployee { get; set; }
        public string? EmployeeCode { get; set; }
        public bool IsCoreMember { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? DigitalSignaturePath { get; set; }
        public int? ClientId { get; set; }
        public string? ClientName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
}


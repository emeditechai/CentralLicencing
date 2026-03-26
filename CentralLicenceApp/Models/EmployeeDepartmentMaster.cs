using System;

namespace CentralLicenceApp.Models
{
    public class EmployeeDepartmentMaster
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}
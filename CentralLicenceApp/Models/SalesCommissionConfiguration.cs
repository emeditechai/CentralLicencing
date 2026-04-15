using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class SalesCommissionConfiguration
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("^(Percentage|FixedAmount)$", ErrorMessage = "Commission type must be Percentage or FixedAmount.")]
        public string CommissionType { get; set; } = "Percentage";

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Default rate must be greater than zero.")]
        public decimal DefaultRate { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Display
        public string? UserName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? DesignationName { get; set; }
    }
}

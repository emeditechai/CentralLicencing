using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class SalesCommissionRule
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [RegularExpression("^(Percentage|FixedAmount)$", ErrorMessage = "Commission type must be Percentage or FixedAmount.")]
        public string CommissionType { get; set; } = "Percentage";

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Rate must be greater than zero.")]
        public decimal Rate { get; set; }

        public int Priority { get; set; }  // Auto-computed: Product=10, Default=0

        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.Today;

        public bool IsActive { get; set; } = true;
        public int CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }

        // Display
        public string? UserName { get; set; }
        public string? ProductName { get; set; }

        public string RateSourceDisplay =>
            ProductId.HasValue ? $"Product: {ProductName}" : "Default";

        public string CommissionTypeDisplay =>
            CommissionType == "Percentage" ? $"{Rate}%" : $"₹{Rate:N2}";
    }
}

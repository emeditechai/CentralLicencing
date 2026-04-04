using System;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class BankMaster
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string BankName { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string BranchName { get; set; } = string.Empty;

        [Required, MaxLength(11)]
        public string IFSCCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? UpiId { get; set; }

        [MaxLength(150)]
        public string? UpiHolderName { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}

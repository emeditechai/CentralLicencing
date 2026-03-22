using System;
using System.ComponentModel.DataAnnotations;

namespace CentralLicenceApp.Models
{
    public class EmailTemplate
    {
        public int Id { get; set; }

        public string TemplateKey { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Body { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

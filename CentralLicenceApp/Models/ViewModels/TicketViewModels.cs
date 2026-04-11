using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CentralLicenceApp.Models.ViewModels
{
    // ── Ticket Category ──
    public class TicketCategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    // ── Ticket Priority ──
    public class TicketPriorityFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Priority Name is required")]
        [Display(Name = "Priority Name")]
        [StringLength(50)]
        public string PriorityName { get; set; } = string.Empty;

        [Display(Name = "Color Code")]
        [StringLength(20)]
        public string? ColorCode { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }

        [Display(Name = "SLA Response Time (Hours)")]
        public int? SlaResponseHours { get; set; }

        [Display(Name = "SLA Resolution Time (Hours)")]
        public int? SlaResolutionHours { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    // ── Ticket Sub Category ──
    public class TicketSubCategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Sub Category Name is required")]
        [Display(Name = "Sub Category Name")]
        [StringLength(100)]
        public string SubCategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public List<TicketCategoryMaster> Categories { get; set; } = new();
    }

    // ── Create Ticket ──
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "Subject is required")]
        [Display(Name = "Subject")]
        [StringLength(300)]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int? SubCategoryId { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public int PriorityId { get; set; }

        [Display(Name = "Attachments")]
        public List<IFormFile>? Attachments { get; set; }

        // Lookups for dropdowns
        public List<TicketCategoryMaster> Categories { get; set; } = new();
        public List<TicketSubCategoryMaster> SubCategories { get; set; } = new();
        public List<TicketPriorityMaster> Priorities { get; set; } = new();
    }

    // ── Ticket Detail (for the details/conversation view) ──
    public class TicketDetailViewModel
    {
        public HelpDeskTicket Ticket { get; set; } = new();
        public List<TicketMessage> Messages { get; set; } = new();
        public List<TicketAttachment> Attachments { get; set; } = new();
        public List<TicketAuditLog> AuditLogs { get; set; } = new();

        // For the reply form
        public string ReplyMessage { get; set; } = string.Empty;
        public bool IsInternal { get; set; }
        public List<IFormFile>? ReplyAttachments { get; set; }

        // For assignment dropdown
        public List<AgentOption> Agents { get; set; } = new();

        // Current user info
        public bool IsAgent { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class AgentOption
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    // ── Ticket List (Index) ──
    public class TicketListViewModel
    {
        public List<HelpDeskTicket> Tickets { get; set; } = new();
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public int? CategoryFilter { get; set; }
        public int? AssignedToFilter { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        // Summary stats
        public int TotalCount { get; set; }
        public int OpenCount { get; set; }
        public int InProgressCount { get; set; }
        public int WaitingCount { get; set; }
        public int ResolvedCount { get; set; }
        public int ClosedCount { get; set; }
    }
}

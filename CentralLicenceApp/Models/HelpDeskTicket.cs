namespace CentralLicenceApp.Models
{
    public class HelpDeskTicket
    {
        public int Id { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int PriorityId { get; set; }
        public string Status { get; set; } = "Open";
        public int CreatedById { get; set; }
        public int? AssignedToId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? FirstResponseAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

        // Navigation / display helpers (populated by JOIN queries)
        public string CategoryName { get; set; } = string.Empty;
        public string? SubCategoryName { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public string? PriorityColor { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public string? AssignedToName { get; set; }

        public int? FinancialYearId { get; set; }
        public string? FYCode { get; set; }
    }
}

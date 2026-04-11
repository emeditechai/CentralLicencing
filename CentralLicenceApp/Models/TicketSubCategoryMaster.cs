namespace CentralLicenceApp.Models
{
    public class TicketSubCategoryMaster
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string SubCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation (populated by JOIN)
        public string CategoryName { get; set; } = string.Empty;
    }
}

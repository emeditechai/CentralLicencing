namespace CentralLicenceApp.Models
{
    public class TicketCannedResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? CreatedById { get; set; }
        public bool IsGlobal { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

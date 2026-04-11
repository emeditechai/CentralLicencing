namespace CentralLicenceApp.Models
{
    public class TicketPriorityMaster
    {
        public int Id { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public string? ColorCode { get; set; }
        public int SortOrder { get; set; }
        public int? SlaResponseHours { get; set; }
        public int? SlaResolutionHours { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

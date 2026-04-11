namespace CentralLicenceApp.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int? MessageId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long? FileSize { get; set; }
        public int UploadedById { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

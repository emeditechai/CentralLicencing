using System;

namespace CentralLicenceApp.Models
{
    public class ExpenseRequestLineAttachment
    {
        public int Id { get; set; }
        public int RequestLineId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
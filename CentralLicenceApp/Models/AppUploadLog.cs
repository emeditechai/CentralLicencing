using System;

namespace CentralLicenceApp.Models
{
    public class AppUploadLog
    {
        public int Id { get; set; }
        public string Platform { get; set; } = string.Empty;       // "Android" or "iOS"
        public string FileName { get; set; } = string.Empty;       // stored file name on disk
        public string OriginalName { get; set; } = string.Empty;   // original uploaded file name
        public long FileSizeBytes { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string? Notes { get; set; }
    }
}

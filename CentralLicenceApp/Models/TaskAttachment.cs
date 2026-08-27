namespace CentralLicenceApp.Models
{
    public class TaskAttachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        /// <summary>Stored file name on disk (GUID-based).</summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>Original file name supplied by the user.</summary>
        public string OriginalName { get; set; } = string.Empty;
        /// <summary>Relative web path, e.g. /uploads/task-attachments/5/abc.pdf</summary>
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int UploadedById { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation / display
        public string UploadedByName { get; set; } = string.Empty;
    }
}

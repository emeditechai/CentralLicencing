using System;

namespace CentralLicenceApp.Models
{
    public class PermissionMaster
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public static class Permissions
    {
        public const string View    = "View";
        public const string Create  = "Create";
        public const string Edit    = "Edit";
        public const string Delete  = "Delete";
        public const string Approve = "Approve";
        public const string Reject  = "Reject";
        public const string Export  = "Export";
        public const string Print   = "Print";
        public const string Cancel  = "Cancel";
        public const string Refund  = "Refund";
        public const string Settle  = "Settle";
        public const string Reimburse = "Reimburse";
    }
}

using System;

namespace CentralLicenceApp.Models
{
    public class UserPermissionMap
    {
        public int UserId { get; set; }
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        public bool IsGranted { get; set; } = true; // true=Allow override, false=Deny override
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
    }
}

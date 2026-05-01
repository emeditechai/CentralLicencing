using System;

namespace CentralLicenceApp.Models
{
    public class RolePermissionMap
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

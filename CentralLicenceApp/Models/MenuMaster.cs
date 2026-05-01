using System;
using System.Collections.Generic;

namespace CentralLicenceApp.Models
{
    public class MenuMaster
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string MenuType { get; set; } = "Link"; // Section | Collapsible | Link
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? IconClass { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Render-time helpers (not persisted)
        public List<MenuMaster> Children { get; set; } = new();
        public bool IsExpanded { get; set; }
        public bool IsActiveItem { get; set; }
    }
}

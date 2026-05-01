using System.Collections.Generic;

namespace CentralLicenceApp.Models.ViewModels
{
    public class MenuUpsertViewModel
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string MenuType { get; set; } = "Link";
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? IconClass { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> PermissionIds { get; set; } = new();
    }

    public class RolePermissionGridRow
    {
        public MenuMaster Menu { get; set; } = new();
        public int Depth { get; set; }
        public List<PermissionMaster> Permissions { get; set; } = new();
        public HashSet<int> Granted { get; set; } = new();
    }

    public class RolePermissionGridViewModel
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<RoleMaster> Roles { get; set; } = new();
        public List<RolePermissionGridRow> Rows { get; set; } = new();
        public List<PermissionMaster> AllPermissions { get; set; } = new();
    }

    public class UserPermissionGridRow
    {
        public MenuMaster Menu { get; set; } = new();
        public int Depth { get; set; }
        public List<PermissionMaster> Permissions { get; set; } = new();
        // Cell state: "Inherit" | "Allow" | "Deny"
        public Dictionary<int, string> CellState { get; set; } = new();
        // Inherited (role) grant — for rendering badge under Inherit cells
        public HashSet<int> RoleGranted { get; set; } = new();
    }

    public class UserPermissionGridViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<UserMaster> Users { get; set; } = new();
        public List<RoleMaster> Roles { get; set; } = new();
        public List<UserPermissionGridRow> Rows { get; set; } = new();
        public List<PermissionMaster> AllPermissions { get; set; } = new();
    }

    public class EffectivePermissionSet
    {
        // Key: MenuId, Value: set of PermissionKey strings granted to user under active role
        public Dictionary<int, HashSet<string>> ByMenuId { get; set; } = new();
        // Key: "Controller/Action" (or "Controller/*"), Value: same set, for fast lookup
        public Dictionary<string, HashSet<string>> ByRoute { get; set; } =
            new(System.StringComparer.OrdinalIgnoreCase);
        // Set of menu IDs that should appear in the sidebar (View granted)
        public HashSet<int> VisibleMenuIds { get; set; } = new();
    }
}

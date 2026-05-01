using System;
using System.Collections.Generic;
using System.Linq;

namespace CentralLicenceApp.Services
{
    /// <summary>
    /// Offline generator for the dynamic-authorization seed SQL scripts
    /// (078_SeedPermissionMaster.sql, 079_SeedMenuMaster.sql, 080_SeedRolePermissionMap.sql).
    /// Does NOT execute any SQL at runtime. Invoke via:
    ///     dotnet run -- emit-auth-sql [outputDir]
    /// </summary>
    public static class DynamicAuthSeeder
    {
        private static readonly (string Key, string Display, int Sort)[] DefaultPermissions =
        {
            ("View",    "View",      10),
            ("Create",  "Create",    20),
            ("Edit",    "Edit",      30),
            ("Delete",  "Delete",    40),
            ("Approve", "Approve",   50),
            ("Reject",  "Reject",    60),
            ("Export",  "Export",    70),
            ("Print",   "Print",     80),
            ("Cancel",  "Cancel",    90),
            ("Refund",  "Refund",   100),
            ("Settle",  "Settle",   110),
            ("Reimburse","Reimburse",120),
        };

        // Definition of a menu node with optional children.
        private record MenuDef(
            string Name,
            string Type,                  // Section | Collapsible | Link
            string? Controller,
            string? Action,
            string? Icon,
            string[] Permissions,         // permission keys this menu supports (View always implied)
            string[] Roles,               // roles allowed (granted all listed permissions)
            MenuDef[]? Children = null);

        // Master menu tree replicating the static layout, with the new "Security" group
        // under Administration. Roles correspond to RoleMaster.RoleName.
        private static readonly string[] R_Admin = { "Administrator" };
        private static readonly string[] R_AdminFinance = { "Administrator", "Finance" };
        private static readonly string[] R_AdminStaff = { "Administrator", "Staff" };
        private static readonly string[] R_AdminStaffFinance = { "Administrator", "Staff", "Finance" };
        private static readonly string[] R_AllNonClient = { "Administrator", "Staff", "Finance" };
        private static readonly string[] R_TicketAdminAgent = { "Administrator", "Ticket Admin", "Ticket Agent" };
        private static readonly string[] R_TicketAdmin = { "Administrator", "Ticket Admin" };
        private static readonly string[] R_AllAuth = { "Administrator", "Staff", "Finance", "Ticket Admin", "Ticket Agent", "ClientTicket" };
        private static readonly string[] CrudPerms = { "View", "Create", "Edit", "Delete" };
        private static readonly string[] ViewExport = { "View", "Export" };
        private static readonly string[] ViewOnly = { "View" };
        private static readonly string[] ViewApprove = { "View", "Approve", "Reject" };

        private static MenuDef[] BuildTree() => new[]
        {
            // ── Main ──
            new MenuDef("Main", "Section", null, null, null, ViewOnly, R_AllNonClient,
                Children: new[]
                {
                    new MenuDef("Dashboard", "Link", "Dashboard", "Index", "bi bi-speedometer2", ViewOnly, R_AllNonClient),
                }),

            // ── Licences ──
            new MenuDef("Licences", "Collapsible", null, null, "bi bi-card-checklist", ViewOnly, R_AllNonClient,
                Children: new[]
                {
                    new MenuDef("Client Licences",   "Link", "ClientLicense",   "Index", "bi bi-card-checklist", CrudPerms, R_AllNonClient),
                    new MenuDef("Validation History","Link", "LicenseHistory",  "Index", "bi bi-clock-history",  ViewExport, R_AllNonClient),
                    new MenuDef("Audit Log",         "Link", "LicenseAuditLog", "Index", "bi bi-clipboard2-pulse", ViewExport, R_AllNonClient),
                }),

            // ── Administration ──
            new MenuDef("Administration", "Collapsible", null, null, "bi bi-sliders2-vertical", ViewOnly, R_Admin,
                Children: new[]
                {
                    new MenuDef("User Management",   "Link", "User",            "Index", "bi bi-people",       CrudPerms, R_Admin),
                    new MenuDef("Role Management",   "Link", "Role",            "Index", "bi bi-diagram-3",    CrudPerms, R_Admin),
                    new MenuDef("Company Settings",  "Link", "CompanySettings", "Index", "bi bi-buildings",    new[]{"View","Edit"}, R_Admin),
                    new MenuDef("Master", "Collapsible", null, null, "bi bi-collection", ViewOnly, R_Admin,
                        Children: new[]
                        {
                            new MenuDef("Employee Department",  "Link", "EmployeeDepartment",  "Index", "bi bi-building",        CrudPerms, R_Admin),
                            new MenuDef("Employee Designation", "Link", "EmployeeDesignation", "Index", "bi bi-person-workspace", CrudPerms, R_Admin),
                            new MenuDef("Financial Year",       "Link", "FinancialYearMaster", "Index", "bi bi-calendar-range",  CrudPerms, R_Admin),
                        }),
                    new MenuDef("Email Engine",      "Link", "EmailConfig",   "Index", "bi bi-send-check",       new[]{"View","Edit"}, R_Admin),
                    new MenuDef("Email Templates",   "Link", "EmailTemplate", "Index", "bi bi-file-earmark-text", CrudPerms, R_Admin),
                    new MenuDef("Email Logs",        "Link", "EmailLog",      "Index", "bi bi-journal-text",      ViewExport, R_Admin),
                    new MenuDef("App File Upload",   "Link", "AppUpload",     "Index", "bi bi-cloud-upload",      CrudPerms, R_Admin),
                    // ── New: Security group ──
                    new MenuDef("Security", "Collapsible", null, null, "bi bi-shield-lock", ViewOnly, R_Admin,
                        Children: new[]
                        {
                            new MenuDef("Menu Management",    "Link", "MenuManagement",    "Index", "bi bi-list-nested",      CrudPerms, R_Admin),
                            new MenuDef("Role Permissions",   "Link", "RolePermission",    "Index", "bi bi-diagram-2",        new[]{"View","Edit"}, R_Admin),
                            new MenuDef("User Permissions",   "Link", "UserPermission",    "Index", "bi bi-person-check",     new[]{"View","Edit"}, R_Admin),
                        }),
                }),

            // ── Business Unit ──
            new MenuDef("Business Unit", "Collapsible", null, null, "bi bi-diagram-2", ViewOnly, R_AllNonClient,
                Children: new[]
                {
                    new MenuDef("Expenses & Advance", "Collapsible", null, null, "bi bi-arrow-left-right", ViewOnly, R_AllNonClient,
                        Children: new[]
                        {
                            new MenuDef("Expense Requests", "Link", "ExpenseRequest", "Index",     "bi bi-wallet2",     new[]{"View","Create","Edit","Delete"}, R_AllNonClient),
                            new MenuDef("Approval Inbox",   "Link", "ExpenseRequest", "Approvals", "bi bi-check2-square", ViewApprove, R_AllNonClient),
                            new MenuDef("Reimbursement Desk","Link","ExpenseRequest", "FinanceDesk","bi bi-cash-stack",  new[]{"View","Reimburse","Settle"}, R_AdminFinance),
                        }),
                    new MenuDef("Quotation & Invoices", "Collapsible", null, null, "bi bi-file-earmark-text", ViewOnly, R_AllNonClient,
                        Children: new[]
                        {
                            new MenuDef("Quotations",      "Link", "Quotation",      "Index", "bi bi-file-earmark-check", new[]{"View","Create","Edit","Cancel","Print"}, R_AllNonClient),
                            new MenuDef("Invoices",        "Link", "Invoice",        "Index", "bi bi-receipt-cutoff",     new[]{"View","Create","Edit","Cancel","Print","Refund"}, R_AllNonClient),
                            new MenuDef("Payment Process", "Link", "InvoicePayment", "Index", "bi bi-cash-coin",          new[]{"View","Create","Edit","Refund"}, R_AllNonClient),
                        }),
                    new MenuDef("Master", "Collapsible", null, null, "bi bi-collection", ViewOnly, R_Admin,
                        Children: new[]
                        {
                            new MenuDef("Expense Category",     "Link", "ExpenseCategory",        "Index", "bi bi-receipt",        CrudPerms, R_Admin),
                            new MenuDef("eProduct Master",      "Link", "ProductMaster",          "Index", "bi bi-box-seam",       CrudPerms, R_Admin),
                            new MenuDef("Product Rate List",    "Link", "ProductRate",            "Index", "bi bi-tags",           CrudPerms, R_Admin),
                            new MenuDef("Party Master",         "Link", "PartyMaster",            "Index", "bi bi-people",         CrudPerms, R_Admin),
                            new MenuDef("Bank Master",          "Link", "BankMaster",             "Index", "bi bi-bank2",          CrudPerms, R_Admin),
                            new MenuDef("Payment Modes",        "Link", "PaymentMode",            "Index", "bi bi-credit-card-2-front", CrudPerms, R_Admin),
                            new MenuDef("Terms & Conditions",   "Link", "TermsConditionTemplate", "Index", "bi bi-file-text",      CrudPerms, R_Admin),
                        }),
                    new MenuDef("Task Payout", "Collapsible", null, null, "bi bi-gear", ViewOnly, R_AdminFinance,
                        Children: new[]
                        {
                            new MenuDef("Payout Configuration", "Link", "PayoutConfiguration", "Index",         "bi bi-sliders",     CrudPerms, R_Admin),
                            new MenuDef("Payout Batches",       "Link", "PayoutBatch",         "Index",         "bi bi-collection",  new[]{"View","Create","Edit"}, R_AdminFinance),
                            new MenuDef("Approval Inbox",       "Link", "PayoutBatch",         "ApprovalInbox", "bi bi-inbox",       ViewApprove, R_AdminFinance),
                            new MenuDef("Settlement Desk",      "Link", "PayoutBatch",         "SettlementDesk","bi bi-cash-stack",  new[]{"View","Settle"}, R_AdminFinance),
                        }),
                    new MenuDef("Sales Commission", "Collapsible", null, null, "bi bi-percent", ViewOnly, R_AdminFinance,
                        Children: new[]
                        {
                            new MenuDef("Commission Config",     "Link", "SalesCommissionConfig",   "Index",         "bi bi-sliders",    CrudPerms, R_Admin),
                            new MenuDef("Invoice Assignment",    "Link", "SalesInvoiceAssignment",  "Index",         "bi bi-link-45deg", CrudPerms, R_Admin),
                            new MenuDef("Commission Batches",    "Link", "SalesCommissionBatch",    "Index",         "bi bi-collection", new[]{"View","Create","Edit"}, R_AdminFinance),
                            new MenuDef("Approval Inbox",        "Link", "SalesCommissionBatch",    "ApprovalInbox", "bi bi-inbox",      ViewApprove, R_AdminFinance),
                            new MenuDef("Settlement Desk",       "Link", "SalesCommissionBatch",    "SettlementDesk","bi bi-cash-stack", new[]{"View","Settle"}, R_AdminFinance),
                        }),
                }),

            // ── Reports ──
            new MenuDef("Reports", "Collapsible", null, null, "bi bi-bar-chart-line", ViewOnly, R_AllNonClient,
                Children: new[]
                {
                    new MenuDef("Client Details",            "Link", "Reports",               "ClientDetails",            "bi bi-file-earmark-bar-graph", ViewExport, R_AllNonClient),
                    new MenuDef("Expense Report",            "Link", "Reports",               "ExpenseReport",            "bi bi-wallet2",                ViewExport, R_AllNonClient),
                    new MenuDef("Settlement Report",         "Link", "Reports",               "SettlementReport",         "bi bi-cash-stack",             ViewExport, R_AllNonClient),
                    new MenuDef("Daily Collection Register", "Link", "Reports",               "DailyCollectionRegister",  "bi bi-cash-coin",              ViewExport, R_AllNonClient),
                    new MenuDef("Client Due Report",         "Link", "Reports",               "ClientDueReport",          "bi bi-exclamation-triangle",   ViewExport, R_AllNonClient),
                    new MenuDef("Summary Report",            "Link", "PayoutReport",          "Summary",                  "bi bi-bar-chart",              ViewExport, R_AllNonClient),
                    new MenuDef("Detail Report",             "Link", "PayoutReport",          "Detail",                   "bi bi-list-columns-reverse",   ViewExport, R_AllNonClient),
                    new MenuDef("History Report",            "Link", "PayoutReport",          "History",                  "bi bi-clock-history",          ViewExport, R_AllNonClient),
                    new MenuDef("Sales Comm Summary",        "Link", "SalesCommissionReport", "Summary",                  "bi bi-percent",                ViewExport, R_AllNonClient),
                    new MenuDef("Sales Comm Detail",         "Link", "SalesCommissionReport", "Detail",                   "bi bi-list-columns-reverse",   ViewExport, R_AllNonClient),
                    new MenuDef("Sales Comm History",        "Link", "SalesCommissionReport", "History",                  "bi bi-clock-history",          ViewExport, R_AllNonClient),
                }),

            // ── CRM ──
            new MenuDef("CRM", "Collapsible", null, null, "bi bi-headset", ViewOnly, R_AllAuth,
                Children: new[]
                {
                    new MenuDef("My Tickets",        "Link", "HelpDeskTicket", "MyTickets", "bi bi-ticket-perforated", new[]{"View","Create","Edit"}, R_AllAuth),
                    new MenuDef("Ticket Management", "Link", "HelpDeskTicket", "Index",     "bi bi-kanban",            new[]{"View","Create","Edit","Delete","Approve"}, R_TicketAdminAgent),
                    new MenuDef("My Task Log",       "Link", "DailyTaskLog",   "Index",     "bi bi-journal-check",     new[]{"View","Create","Edit"}, R_TicketAdminAgent),
                    new MenuDef("Team Task Log",     "Link", "DailyTaskLog",   "TeamView",  "bi bi-people",            ViewExport, R_TicketAdmin),
                    new MenuDef("Master", "Collapsible", null, null, "bi bi-collection", ViewOnly, R_TicketAdmin,
                        Children: new[]
                        {
                            new MenuDef("Ticket Categories",     "Link", "TicketCategory",     "Index", "bi bi-tags",          CrudPerms, R_TicketAdmin),
                            new MenuDef("Ticket Sub Categories", "Link", "TicketSubCategory",  "Index", "bi bi-bookmark",      CrudPerms, R_TicketAdmin),
                            new MenuDef("Ticket Priorities",     "Link", "TicketPriority",     "Index", "bi bi-flag",          CrudPerms, R_TicketAdmin),
                            new MenuDef("Task Types",            "Link", "TaskTypeMaster",     "Index", "bi bi-code-slash",    CrudPerms, R_TicketAdmin),
                            new MenuDef("Task Categories",       "Link", "TaskCategoryMaster", "Index", "bi bi-bookmark-star", CrudPerms, R_TicketAdmin),
                            new MenuDef("Project / Module",      "Link", "ProjectModule",      "Index", "bi bi-folder2-open",  CrudPerms, R_TicketAdmin),
                        }),
                    new MenuDef("Reports", "Collapsible", null, null, "bi bi-bar-chart-line", ViewOnly, R_TicketAdminAgent,
                        Children: new[]
                        {
                            new MenuDef("Analytics Dashboard",   "Link", "TicketReports",  "Dashboard",            "bi bi-speedometer2", ViewOnly,   R_TicketAdmin),
                            new MenuDef("Agent Performance",     "Link", "TicketReports",  "AgentPerformance",     "bi bi-people",       ViewExport, R_TicketAdminAgent),
                            new MenuDef("SLA Compliance",        "Link", "TicketReports",  "SlaCompliance",        "bi bi-shield-check", ViewExport, R_TicketAdmin),
                            new MenuDef("Timesheet Report",      "Link", "TaskReport",     "Timesheet",            "bi bi-calendar3-week", ViewExport, R_TicketAdminAgent),
                            new MenuDef("Employee Productivity", "Link", "TaskReport",     "EmployeeProductivity", "bi bi-graph-up-arrow", ViewExport, R_TicketAdminAgent),
                            new MenuDef("Project Effort",        "Link", "TaskReport",     "ProjectEffort",        "bi bi-folder2-open",   ViewExport, R_TicketAdminAgent),
                            new MenuDef("Payout Summary",        "Link", "PayoutReport",   "Summary",              "bi bi-cash-stack",     ViewExport, R_TicketAdminAgent),
                            new MenuDef("Payout Detail",         "Link", "PayoutReport",   "Detail",               "bi bi-receipt",        ViewExport, R_TicketAdminAgent),
                            new MenuDef("Payout History",        "Link", "PayoutReport",   "History",              "bi bi-clock-history",  ViewExport, R_TicketAdminAgent),
                            new MenuDef("CRM Sales Comm Summary","Link", "SalesCommissionReport", "Summary",       "bi bi-percent",        ViewExport, R_TicketAdminAgent),
                            new MenuDef("CRM Sales Comm Detail", "Link", "SalesCommissionReport", "Detail",        "bi bi-receipt",        ViewExport, R_TicketAdminAgent),
                            new MenuDef("CRM Sales Comm History","Link", "SalesCommissionReport", "History",       "bi bi-clock-history",  ViewExport, R_TicketAdminAgent),
                        }),
                }),
        };

        // ============================================================
        // SQL Script Generators (offline; emit static .sql files)
        // ============================================================
        public static void EmitSqlScripts(string outputDir)
        {
            System.IO.Directory.CreateDirectory(outputDir);
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "078_SeedPermissionMaster.sql"), BuildPermissionsSql());
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "079_SeedMenuMaster.sql"), BuildMenusSql());
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "080_SeedRolePermissionMap.sql"), BuildRoleGrantsSql());
        }

        private static string BuildPermissionsSql()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine("-- 078_SeedPermissionMaster.sql");
            sb.AppendLine("-- Seeds the standard permission keys used by the dynamic authorization layer.");
            sb.AppendLine("-- Idempotent: existing rows are not duplicated.");
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine();
            foreach (var (key, display, sort) in DefaultPermissions)
            {
                sb.AppendLine($"IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'{Esc(key)}')");
                sb.AppendLine($"    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'{Esc(key)}', N'{Esc(display)}', {sort}, 1);");
            }
            sb.AppendLine("GO");
            return sb.ToString();
        }

        private static string BuildMenusSql()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine("-- 079_SeedMenuMaster.sql");
            sb.AppendLine("-- Seeds MenuMaster + MenuPermissionMap to mirror the application sidebar.");
            sb.AppendLine("-- Idempotent: only inserts when MenuMaster is empty.");
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine();
            sb.AppendLine("IF NOT EXISTS (SELECT 1 FROM MenuMaster)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // Declare permission id variables
            foreach (var (key, _, _) in DefaultPermissions)
                sb.AppendLine($"    DECLARE @P_{key} INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'{key}');");
            sb.AppendLine();

            int counter = 0;
            int sort = 0;
            foreach (var root in BuildTree())
                EmitMenuRecursive(sb, root, parentVar: null, sortOrder: sort++, ref counter);

            sb.AppendLine("END");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        private static void EmitMenuRecursive(System.Text.StringBuilder sb, MenuDef def, string? parentVar, int sortOrder, ref int counter)
        {
            counter++;
            var thisVar = $"@M{counter}";
            string parentExpr = parentVar ?? "NULL";
            string ctrl = def.Controller is null ? "NULL" : $"N'{Esc(def.Controller)}'";
            string act = def.Action is null ? "NULL" : $"N'{Esc(def.Action)}'";
            string icon = def.Icon is null ? "NULL" : $"N'{Esc(def.Icon)}'";

            sb.AppendLine($"    DECLARE {thisVar} INT;");
            sb.AppendLine($"    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)");
            sb.AppendLine($"        VALUES ({parentExpr}, N'{Esc(def.Name)}', N'{Esc(def.Type)}', {ctrl}, {act}, {icon}, {sortOrder}, 1);");
            sb.AppendLine($"    SET {thisVar} = SCOPE_IDENTITY();");

            // Permissions
            var permKeys = def.Permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!permKeys.Contains("View", StringComparer.OrdinalIgnoreCase)) permKeys.Insert(0, "View");
            foreach (var pk in permKeys)
                sb.AppendLine($"    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES ({thisVar}, @P_{pk});");
            sb.AppendLine();

            if (def.Children != null)
            {
                int csort = 0;
                foreach (var c in def.Children)
                    EmitMenuRecursive(sb, c, thisVar, csort++, ref counter);
            }
        }

        private static string BuildRoleGrantsSql()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine("-- 080_SeedRolePermissionMap.sql");
            sb.AppendLine("-- Seeds RolePermissionMap based on the static sidebar's original role rules.");
            sb.AppendLine("-- Idempotent: only inserts when RolePermissionMap is empty.");
            sb.AppendLine("-- Requires 077, 078, 079 to be applied first.");
            sb.AppendLine("-- =========================================================================");
            sb.AppendLine();
            sb.AppendLine("IF NOT EXISTS (SELECT 1 FROM RolePermissionMap)");
            sb.AppendLine("BEGIN");
            sb.AppendLine("    SET NOCOUNT ON;");
            sb.AppendLine();

            // Declare role IDs
            var allRoles = new[] { "Administrator", "Staff", "Finance", "Ticket Admin", "Ticket Agent", "ClientTicket" };
            foreach (var r in allRoles)
                sb.AppendLine($"    DECLARE @R_{RoleVar(r)} INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'{Esc(r)}');");
            sb.AppendLine();

            // Walk tree; for each menu (by path) add grants
            foreach (var root in BuildTree())
                EmitRoleGrantsRecursive(sb, root, parentPath: null);

            sb.AppendLine("END");
            sb.AppendLine("GO");
            return sb.ToString();
        }

        private static void EmitRoleGrantsRecursive(System.Text.StringBuilder sb, MenuDef def, string? parentPath)
        {
            var path = parentPath == null ? def.Name : parentPath + "\u001F" + def.Name;

            var permKeys = def.Permissions.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!permKeys.Contains("View", StringComparer.OrdinalIgnoreCase)) permKeys.Insert(0, "View");

            // Resolve menu id by walking path: locate menu where MenuName = leaf and parent chain matches.
            // We emit a CTE-based lookup using ParentId chain.
            var menuLookup = BuildMenuLookupSql(path);

            foreach (var role in def.Roles)
            {
                foreach (var pk in permKeys)
                {
                    sb.AppendLine($"    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)");
                    sb.AppendLine($"        SELECT @R_{RoleVar(role)}, {menuLookup}, (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'{pk}')");
                    sb.AppendLine($"        WHERE @R_{RoleVar(role)} IS NOT NULL");
                    sb.AppendLine($"          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_{RoleVar(role)} AND MenuId = {menuLookup} AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'{pk}'));");
                }
            }
            sb.AppendLine();

            if (def.Children != null)
                foreach (var c in def.Children)
                    EmitRoleGrantsRecursive(sb, c, path);
        }

        private static string BuildMenuLookupSql(string path)
        {
            // Build subquery walking the menu tree by name path.
            // Path "Administration/Security/Menu Management" → leaf 'Menu Management', parent 'Security' (parent 'Administration' (root)).
            var parts = path.Split('\u001F');
            // Innermost: SELECT Id FROM MenuMaster WHERE MenuName='leaf' AND ParentId = (parent lookup)
            string current = "NULL";
            for (int i = 0; i < parts.Length; i++)
            {
                var name = Esc(parts[i]);
                if (i == 0)
                {
                    current = $"(SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'{name}' AND ParentId IS NULL)";
                }
                else
                {
                    current = $"(SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'{name}' AND ParentId = {current})";
                }
            }
            return current;
        }

        private static string RoleVar(string role) => role.Replace(" ", "_");
        private static string Esc(string s) => s.Replace("'", "''");
    }
}

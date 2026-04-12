# Plan: Dynamic Authorization & DB-Driven Menu System

## TL;DR
Replace hardcoded `[Authorize(Roles = "...")]` attributes and static Razor sidebar with a fully DB-driven authorization and menu system. New tables `MenuMaster` (parent-child menu tree) and `RoleMenuMap` (role→menu access) drive both sidebar rendering and page-level authorization. Admin UI pages allow managing menus and role-menu assignments. Super-admin bypass preserved. Single active role model preserved. Sidebar visual appearance unchanged.

## Decisions
- Super-admin bypass preserved (admin user = god mode)
- Single active role model preserved (user picks one role at a time)
- Admin UI pages under Administration section
- ~58 existing menu items auto-seeded into MenuMaster with RoleMenuMap pre-populated
- No business logic changes, no sidebar CSS/visual changes
- `[Authorize(Roles = "...")]` replaced with `[Authorize]` + dynamic filter
- Imperative `User.IsInRole()` checks in action bodies kept (data-scoping logic)

---

## Phase 1: Database Schema, Models & Seeding (Steps 1-3)

### Step 1 — SQL migration script
**File**: `SqlScripts/065_CreateMenuAndRoleMenuMap.sql`

**MenuMaster table:**
| Column | Type | Notes |
|---|---|---|
| Id | INT IDENTITY PK | |
| ParentId | INT NULL FK→MenuMaster(Id) | Self-referencing for parent-child |
| MenuName | NVARCHAR(100) | Display text |
| MenuType | NVARCHAR(20) | 'Section' / 'Collapsible' / 'Link' |
| ControllerName | NVARCHAR(100) NULL | e.g. "Invoice" (NULL for Section/Collapsible) |
| ActionName | NVARCHAR(100) NULL | e.g. "Index" (NULL if default) |
| IconClass | NVARCHAR(50) NULL | e.g. "bi bi-speedometer2" |
| SortOrder | INT DEFAULT 0 | Ordering within siblings |
| IsActive | BIT DEFAULT 1 | Soft disable |
| CreatedAt | DATETIME DEFAULT GETDATE() | |

**RoleMenuMap table:**
| Column | Type | Notes |
|---|---|---|
| RoleId | INT FK→RoleMaster(Id) | |
| MenuId | INT FK→MenuMaster(Id) ON DELETE CASCADE | |
| CreatedAt | DATETIME DEFAULT GETDATE() | |
| PK | (RoleId, MenuId) composite | |

### Step 2 — DatabaseSeeder update
**File**: `Services/DatabaseSeeder.cs`
- Create tables idempotently (IF NOT EXISTS)
- Seed ~58 menu items (6 sections + 6 collapsibles + ~46 links + 2 new admin pages)
- Seed RoleMenuMap from current visibility rules:
  - **Administrator**: All menus
  - **Staff**: MAIN, LICENCES, BUSINESS UNIT (excl Reimbursement Desk), REPORTS, CRM→My Tickets
  - **Finance**: Same as Staff + Reimbursement Desk
  - **Ticket Admin**: CRM (all items + Master + all Reports)
  - **Ticket Agent**: CRM limited (no Master, no Analytics Dashboard, no SLA Compliance)
  - **ClientTicket**: CRM→My Tickets only

### Step 3 — C# models
**Files**: `Models/MenuMaster.cs`, `Models/RoleMenuMap.cs`
- MenuMaster: Properties matching table columns + `List<MenuMaster> Children` (populated in memory) + `bool IsExpanded` / `bool IsActiveItem` (set at render time)
- RoleMenuMap: `RoleId`, `MenuId`, `CreatedAt`

---

## Phase 2: Repository Layer (Step 4)

### Step 4 — IMenuRepository + MenuRepository
**Files**: `Repositories/IMenuRepository.cs`, `Repositories/MenuRepository.cs`

**Interface methods:**
- `GetAllAsync()` — all menus ordered by SortOrder
- `GetByIdAsync(int id)`
- `CreateAsync(MenuMaster menu)`
- `UpdateAsync(MenuMaster menu)`
- `ValidateDeleteAsync(int id)` — check children/references
- `DeleteAsync(int id)`
- `GetMenusForRoleAsync(int roleId)` — JOIN RoleMenuMap
- `GetMenuIdsForRoleAsync(int roleId)` — for mapping UI
- `SaveRoleMenuMappingAsync(int roleId, List<int> menuIds)` — full replace (delete all + re-insert)

**Pattern**: Follow existing repo pattern — `CreateConnection() → SqlConnection`, inline SQL with Dapper, transactions for multi-statement operations.

---

## Phase 3: Permission Service — Cached (Step 5)

### Step 5 — IPermissionService + PermissionService
**Files**: `Services/IPermissionService.cs`, `Services/PermissionService.cs`

**Interface:**
- `GetMenuTreeForRoleAsync(int roleId)` → hierarchical tree with only accessible items
- `HasPageAccessAsync(int roleId, string controller, string? action)` → bool
- `InvalidateCache(int roleId)` — clears cached data for role
- `InvalidateAllCache()` — clears all cached data

**Implementation:**
- Depends on `IMenuRepository` + `IMemoryCache`
- Cache key: `MenuAccess_{roleId}` → `Dictionary<string, HashSet<string>>` mapping controller→allowed actions
- Cache key: `MenuTree_{roleId}` → `List<MenuMaster>` hierarchical tree
- Cache duration: 30 minutes (configurable)
- `HasPageAccessAsync`: lookup controller in cache dict; if found and action set is empty → all actions allowed; if action set has entries → check if current action is in set; if controller not in dict → allow (unregistered pages like Account, Home)
- `GetMenuTreeForRoleAsync`: load role's menus, build parent-child tree, prune sections that have no accessible children

---

## Phase 4: Dynamic Menu ViewComponent (Steps 6-8)

### Step 6 — SideMenuViewComponent
**File**: `ViewComponents/SideMenuViewComponent.cs`
- Injects `IPermissionService`
- Gets `ActiveRoleId` from claims, current controller/action from route
- Calls `GetMenuTreeForRoleAsync(roleId)` to get the menu tree
- Marks `IsActiveItem` on the current menu item (match controller+action)
- Marks `IsExpanded` on parents of the active item
- Returns `View(menuTree)`

### Step 7 — ViewComponent template
**File**: `Views/Shared/Components/SideMenu/Default.cshtml`
- **Exact same HTML/CSS structure** as current sidebar — recursive rendering helper for nested menus
- Same CSS classes: `.nav-lbl`, `.nav-lbl-toggle`, `.nav-item`, `.nav-sub-item`, `.nav-sub-item-deep`
- Same Bootstrap 5 collapse behavior (`data-bs-toggle="collapse"`)
- Same icons from `MenuMaster.IconClass`
- Same active/expanded logic from ViewComponent data
- Super admin sees all menus (PermissionService returns all for super admin)

### Step 8 — Update _Layout.cshtml
**File**: `Views/Shared/_Layout.cshtml`
- Replace the entire hardcoded sidebar `<nav>` content with: `@await Component.InvokeAsync("SideMenu")`
- Keep the brand/logo at the top (static)
- Keep the Sign Out button at the bottom (static)
- **All CSS unchanged**
- Remove `isSuperAdmin`/`isAdmin`/`isCrmOnly` variables and all `@if` role-check blocks

---

## Phase 5: Dynamic Authorization Filter (Steps 9-10)

### Step 9 — DynamicPageAuthorizationFilter
**File**: `Filters/DynamicPageAuthorizationFilter.cs`
- Implements `IAsyncAuthorizationFilter` (registered globally)
- Logic:
  1. Skip if user not authenticated → let `[Authorize]` handle redirect
  2. Skip if user is super admin (`IsSuperAdmin` claim) → allow everything
  3. Get `ActiveRoleId` from claims
  4. Get current controller + action from `ActionDescriptor`
  5. Skip if controller is `Account`, `Home`, or `Error` (always allowed)
  6. Call `permissionService.HasPageAccessAsync(roleId, controller, action)`
  7. If allowed → continue; if denied → redirect to `/Account/AccessDenied`

### Step 10 — Register filter + update controllers
**File**: `Program.cs`
- Register `IPermissionService` as scoped DI
- Register `IMenuRepository` as scoped DI
- Add `DynamicPageAuthorizationFilter` as global filter:
  ```csharp
  builder.Services.AddControllersWithViews(options => {
      options.Filters.Add<DynamicPageAuthorizationFilter>();
  });
  ```

**Files**: ALL controllers (~30 files)
- Replace `[Authorize(Roles = "Administrator")]` → `[Authorize]`
- Replace `[Authorize(Roles = "Administrator,Ticket Admin")]` → `[Authorize]`
- Replace all class-level and action-level `Roles = "..."` → just `[Authorize]`
- Keep `[Authorize]` on controllers that need authentication
- Keep `[AllowAnonymous]` on AccountController login/registration
- **Keep `User.IsInRole()` imperative checks** inside controller actions for data-scoping (e.g., "Ticket Agent sees only own data") — these are business logic, not page access

---

## Phase 6: Admin UI Pages (Steps 11-13)

### Step 11 — Menu Management page
**Files**: `Controllers/MenuManagementController.cs`, `Views/MenuManagement/Index.cshtml`
- `[Authorize]` (dynamic filter handles access)
- Index: Tree table showing all menus with indentation for hierarchy
  - Columns: Menu Name, Type, Controller/Action, Icon, Sort Order, Active, Actions
  - Add/Edit via modal form: ParentId (dropdown), MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive
  - Delete with validation (warn if has children or role mappings)

### Step 12 — Role-Menu Mapping page
**Files**: `Controllers/RoleMenuMappingController.cs`, `Views/RoleMenuMapping/Index.cshtml`
- `[Authorize]` (dynamic filter handles access)
- Index: Role selector dropdown at top
- On role select: shows all menus as a checkable tree
  - Sections with child items → parent checkbox (check/uncheck all children)
  - Individual leaf items → individual checkboxes
  - Pre-checked based on existing RoleMenuMap
- Save button: calls `SaveRoleMenuMappingAsync(roleId, checkedMenuIds)` → invalidates cache

### Step 13 — Seed new admin pages into MenuMaster
- Menu Management → MenuManagement/Index (under Administration)
- Role-Menu Mapping → RoleMenuMapping/Index (under Administration)
- Seed with Administrator-only access in RoleMenuMap

---

## Phase 7: Integration, Cache & Cleanup (Steps 14-15)

### Step 14 — Cache invalidation wiring
- RoleMenuMappingController.Save → calls `permissionService.InvalidateCache(roleId)`
- MenuManagementController.Create/Update/Delete → calls `permissionService.InvalidateAllCache()`

### Step 15 — AccountController integration
- No changes needed to `SignInWithRoleAsync` — the `ActiveRoleId` claim is already set
- `SwitchRole` already re-signs in with new role claim — cache will serve new role's menus automatically
- `OnValidatePrincipal` continues validating user/role existence

---

## New Files (14)
| # | File |
|---|---|
| 1 | `SqlScripts/065_CreateMenuAndRoleMenuMap.sql` |
| 2 | `Models/MenuMaster.cs` |
| 3 | `Models/RoleMenuMap.cs` |
| 4 | `Repositories/IMenuRepository.cs` |
| 5 | `Repositories/MenuRepository.cs` |
| 6 | `Services/IPermissionService.cs` |
| 7 | `Services/PermissionService.cs` |
| 8 | `ViewComponents/SideMenuViewComponent.cs` |
| 9 | `Views/Shared/Components/SideMenu/Default.cshtml` |
| 10 | `Filters/DynamicPageAuthorizationFilter.cs` |
| 11 | `Controllers/MenuManagementController.cs` |
| 12 | `Views/MenuManagement/Index.cshtml` |
| 13 | `Controllers/RoleMenuMappingController.cs` |
| 14 | `Views/RoleMenuMapping/Index.cshtml` |

## Modified Files (~33)
| File | Change |
|---|---|
| `Services/DatabaseSeeder.cs` | Table creation + seeding |
| `Program.cs` | DI + global filter |
| `Views/Shared/_Layout.cshtml` | ViewComponent replacement |
| ~30 Controllers | `[Authorize(Roles = "...")]` → `[Authorize]` |

## Files NOT Modified (business logic preserved)
- `AdminClaimsTransformation.cs` — super admin bypass preserved
- `AdminAuthorizationMiddlewareResultHandler.cs` — super admin bypass preserved
- `AccountController.cs` — login/claims/SwitchRole flow unchanged
- All imperative `User.IsInRole()` data-scoping checks inside action bodies
- All CSS / wwwroot assets
- All business logic in controllers

---

## Verification Checklist
1. `dotnet build` succeeds with 0 errors
2. App startup seeds MenuMaster + RoleMenuMap tables without errors
3. **Administrator** → sees all menus, accesses all pages
4. **Staff** → sees MAIN/LICENCES/BUSINESS UNIT/REPORTS/CRM→My Tickets; admin pages → AccessDenied
5. **Ticket Agent** → sees CRM subset only; Dashboard → AccessDenied
6. **ClientTicket** → sees CRM→My Tickets only; everything else → AccessDenied
7. **Super admin** (`admin` user) → sees all, accesses all regardless of role-menu mapping
8. **Menu Management** → CRUD works, changes reflect in sidebar after cache invalidation
9. **Role-Menu Mapping** → save updates permissions; sidebar updates for that role
10. **Switch Role** → sidebar and access permissions update to new role's menus
11. **Visual diff** → sidebar appearance identical to current (same CSS, icons, layout)

---

## Future Considerations
1. **Action-level granular permissions** (e.g., "can view invoices but not create") — layer via PermissionMaster + RolePermissionMap. Defer unless explicitly needed.
2. **User-specific menu overrides** beyond their role — add UserMenuMap table. Defer — standard RBAC through roles should suffice.
3. **Audit logging** for menu/mapping changes — can be added as a follow-up feature.

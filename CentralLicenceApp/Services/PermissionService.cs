using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace CentralLicenceApp.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private readonly IMenuRepository _menuRepo;
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

        // List of (userId, roleId) cache keys, tracked so we can invalidate per-user / per-role.
        private static readonly object _trackerLock = new();
        private static readonly HashSet<(int UserId, int RoleId)> _trackedKeys = new();

        public PermissionService(IConfiguration config, IMemoryCache cache, IMenuRepository menuRepo)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _cache = cache;
            _menuRepo = menuRepo;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public bool IsSuperAdmin(ClaimsPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated != true) return false;
            if (principal.HasClaim("IsSuperAdmin", "true")) return true;
            return string.Equals(principal.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string CacheKey(int userId, int roleId) => $"Perm_{userId}_{roleId}";

        public async Task<EffectivePermissionSet> GetEffectivePermissionsAsync(int userId, int roleId)
        {
            var key = CacheKey(userId, roleId);
            if (_cache.TryGetValue<EffectivePermissionSet>(key, out var cached) && cached != null)
                return cached;

            var built = await BuildAsync(userId, roleId);
            _cache.Set(key, built, CacheTtl);
            lock (_trackerLock) { _trackedKeys.Add((userId, roleId)); }
            return built;
        }

        private async Task<EffectivePermissionSet> BuildAsync(int userId, int roleId)
        {
            using var conn = CreateConnection();

            // Pull all menus + all permissions
            var menus = (await conn.QueryAsync<MenuMaster>(
                "SELECT * FROM MenuMaster WHERE IsActive=1")).ToList();
            var perms = (await conn.QueryAsync<PermissionMaster>(
                "SELECT * FROM PermissionMaster WHERE IsActive=1")).ToList();
            var permById = perms.ToDictionary(p => p.Id, p => p.PermissionKey);

            // Role grants
            var roleGrants = (await conn.QueryAsync<(int MenuId, int PermissionId)>(
                "SELECT MenuId, PermissionId FROM RolePermissionMap WHERE RoleId=@R",
                new { R = roleId })).ToList();

            // User overrides
            var userOver = (await conn.QueryAsync<(int MenuId, int PermissionId, bool IsGranted)>(
                "SELECT MenuId, PermissionId, IsGranted FROM UserPermissionMap WHERE UserId=@U",
                new { U = userId })).ToList();

            // Build effective: start with role, apply overrides
            var effective = new Dictionary<int, HashSet<string>>(); // MenuId -> permKeys
            foreach (var (menuId, permId) in roleGrants)
            {
                if (!permById.TryGetValue(permId, out var pk)) continue;
                if (!effective.TryGetValue(menuId, out var set))
                    effective[menuId] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                set.Add(pk);
            }
            foreach (var (menuId, permId, isGranted) in userOver)
            {
                if (!permById.TryGetValue(permId, out var pk)) continue;
                if (!effective.TryGetValue(menuId, out var set))
                    effective[menuId] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (isGranted) set.Add(pk);
                else set.Remove(pk); // explicit deny
            }

            // Build route lookup
            var byRoute = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            var visible = new HashSet<int>();
            foreach (var m in menus)
            {
                if (!effective.TryGetValue(m.Id, out var set) || set.Count == 0) continue;
                if (set.Contains(Permissions.View)) visible.Add(m.Id);
                if (!string.IsNullOrWhiteSpace(m.ControllerName))
                {
                    var routeKey = $"{m.ControllerName}/{m.ActionName ?? "*"}";
                    byRoute[routeKey] = set;
                    // Also map controller-only fallback
                    var ctrlKey = $"{m.ControllerName}/*";
                    if (!byRoute.ContainsKey(ctrlKey)) byRoute[ctrlKey] = set;
                }
            }

            // Visible parents: any section/collapsible that has a visible descendant
            var byId = menus.ToDictionary(x => x.Id);
            foreach (var m in menus)
            {
                if (!visible.Contains(m.Id)) continue;
                var p = m.ParentId;
                while (p.HasValue && byId.TryGetValue(p.Value, out var parent))
                {
                    visible.Add(parent.Id);
                    p = parent.ParentId;
                }
            }

            return new EffectivePermissionSet
            {
                ByMenuId = effective,
                ByRoute = byRoute,
                VisibleMenuIds = visible
            };
        }

        public async Task<List<MenuMaster>> GetMenuTreeForUserAsync(int userId, int roleId)
        {
            var eff = await GetEffectivePermissionsAsync(userId, roleId);
            using var conn = CreateConnection();
            var all = (await conn.QueryAsync<MenuMaster>(
                "SELECT * FROM MenuMaster WHERE IsActive=1 ORDER BY SortOrder, MenuName")).ToList();
            // Filter to visible
            var visible = all.Where(m => eff.VisibleMenuIds.Contains(m.Id)).ToList();
            // Build tree
            var byId = visible.ToDictionary(x => x.Id);
            var roots = new List<MenuMaster>();
            foreach (var m in visible)
            {
                if (m.ParentId.HasValue && byId.TryGetValue(m.ParentId.Value, out var parent))
                    parent.Children.Add(m);
                else
                    roots.Add(m);
            }
            return roots;
        }

        public async Task<bool> HasPageAccessAsync(int userId, int roleId, string controller, string? action)
            => await HasPermissionAsync(userId, roleId, controller, action, Permissions.View);

        public async Task<bool> HasPermissionAsync(int userId, int roleId, string controller, string? action, string permissionKey)
        {
            var eff = await GetEffectivePermissionsAsync(userId, roleId);
            // Try exact route then controller fallback
            if (eff.ByRoute.TryGetValue($"{controller}/{action}", out var set1) && set1.Contains(permissionKey)) return true;
            if (eff.ByRoute.TryGetValue($"{controller}/*", out var set2) && set2.Contains(permissionKey)) return true;
            if (eff.ByRoute.TryGetValue($"{controller}/Index", out var set3) && set3.Contains(permissionKey)) return true;
            return false;
        }

        public void InvalidateUser(int userId)
        {
            List<(int U, int R)> snapshot;
            lock (_trackerLock) snapshot = _trackedKeys.Where(k => k.UserId == userId).ToList();
            foreach (var k in snapshot)
            {
                _cache.Remove(CacheKey(k.U, k.R));
                lock (_trackerLock) _trackedKeys.Remove(k);
            }
        }

        public void InvalidateRole(int roleId)
        {
            List<(int U, int R)> snapshot;
            lock (_trackerLock) snapshot = _trackedKeys.Where(k => k.RoleId == roleId).ToList();
            foreach (var k in snapshot)
            {
                _cache.Remove(CacheKey(k.U, k.R));
                lock (_trackerLock) _trackedKeys.Remove(k);
            }
        }

        public void InvalidateAll()
        {
            List<(int U, int R)> snapshot;
            lock (_trackerLock) { snapshot = _trackedKeys.ToList(); _trackedKeys.Clear(); }
            foreach (var k in snapshot) _cache.Remove(CacheKey(k.U, k.R));
        }
    }
}

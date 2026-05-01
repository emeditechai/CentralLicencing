using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly string _connectionString;
        public MenuRepository(string connectionString) { _connectionString = connectionString; }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<MenuMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<MenuMaster>(
                "SELECT * FROM MenuMaster ORDER BY SortOrder, MenuName");
        }

        public async Task<MenuMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<MenuMaster>(
                "SELECT * FROM MenuMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(MenuMaster m)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
                VALUES (@ParentId, @MenuName, @MenuType, @ControllerName, @ActionName, @IconClass, @SortOrder, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", m);
        }

        public async Task<bool> UpdateAsync(MenuMaster m)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE MenuMaster SET
                    ParentId=@ParentId, MenuName=@MenuName, MenuType=@MenuType,
                    ControllerName=@ControllerName, ActionName=@ActionName,
                    IconClass=@IconClass, SortOrder=@SortOrder, IsActive=@IsActive
                WHERE Id=@Id", m);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync("DELETE FROM MenuMaster WHERE Id=@Id", new { Id = id });
            return rows > 0;
        }

        public async Task<int> CountChildrenAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM MenuMaster WHERE ParentId=@Id", new { Id = id });
        }

        public async Task<IEnumerable<int>> GetPermissionIdsForMenuAsync(int menuId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<int>(
                "SELECT PermissionId FROM MenuPermissionMap WHERE MenuId=@MenuId",
                new { MenuId = menuId });
        }

        public async Task SetMenuPermissionsAsync(int menuId, IEnumerable<int> permissionIds)
        {
            using var conn = (SqlConnection)CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM MenuPermissionMap WHERE MenuId=@MenuId",
                new { MenuId = menuId }, tx);
            foreach (var pid in permissionIds.Distinct())
            {
                await conn.ExecuteAsync(
                    "INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@MenuId, @PermissionId)",
                    new { MenuId = menuId, PermissionId = pid }, tx);
            }
            tx.Commit();
        }

        public async Task<MenuMaster?> ResolveByRouteAsync(string controller, string? action)
        {
            using var conn = CreateConnection();
            // Prefer exact controller+action match; fall back to controller-only (Index style)
            var exact = await conn.QuerySingleOrDefaultAsync<MenuMaster>(@"
                SELECT TOP 1 * FROM MenuMaster
                WHERE IsActive=1 AND ControllerName=@C AND ActionName=@A",
                new { C = controller, A = action });
            if (exact != null) return exact;

            return await conn.QuerySingleOrDefaultAsync<MenuMaster>(@"
                SELECT TOP 1 * FROM MenuMaster
                WHERE IsActive=1 AND ControllerName=@C AND (ActionName IS NULL OR ActionName='Index')
                ORDER BY CASE WHEN ActionName IS NULL THEN 1 ELSE 0 END",
                new { C = controller });
        }
    }
}

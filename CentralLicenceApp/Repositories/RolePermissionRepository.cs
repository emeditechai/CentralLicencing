using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly string _connectionString;
        public RolePermissionRepository(string c) { _connectionString = c; }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<RolePermissionMap>> GetForRoleAsync(int roleId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<RolePermissionMap>(
                "SELECT * FROM RolePermissionMap WHERE RoleId=@RoleId", new { RoleId = roleId });
        }

        public async Task SaveForRoleAsync(int roleId, IEnumerable<(int MenuId, int PermissionId)> grants)
        {
            using var conn = (SqlConnection)CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM RolePermissionMap WHERE RoleId=@RoleId",
                new { RoleId = roleId }, tx);
            foreach (var g in grants)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
                    VALUES (@RoleId, @MenuId, @PermissionId)",
                    new { RoleId = roleId, g.MenuId, g.PermissionId }, tx);
            }
            tx.Commit();
        }

        public async Task GrantAsync(int roleId, int menuId, int permissionId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId=@RoleId AND MenuId=@MenuId AND PermissionId=@PermissionId)
                    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
                    VALUES (@RoleId, @MenuId, @PermissionId)",
                new { RoleId = roleId, MenuId = menuId, PermissionId = permissionId });
        }
    }
}

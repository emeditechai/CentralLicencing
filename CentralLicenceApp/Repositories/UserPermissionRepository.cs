using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class UserPermissionRepository : IUserPermissionRepository
    {
        private readonly string _connectionString;
        public UserPermissionRepository(string c) { _connectionString = c; }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<UserPermissionMap>> GetForUserAsync(int userId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<UserPermissionMap>(
                "SELECT * FROM UserPermissionMap WHERE UserId=@UserId", new { UserId = userId });
        }

        public async Task SaveForUserAsync(int userId, int? createdBy, IEnumerable<UserPermissionMap> overrides)
        {
            using var conn = (SqlConnection)CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM UserPermissionMap WHERE UserId=@UserId",
                new { UserId = userId }, tx);
            foreach (var o in overrides)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO UserPermissionMap (UserId, MenuId, PermissionId, IsGranted, CreatedBy)
                    VALUES (@UserId, @MenuId, @PermissionId, @IsGranted, @CreatedBy)",
                    new { UserId = userId, o.MenuId, o.PermissionId, o.IsGranted, CreatedBy = createdBy }, tx);
            }
            tx.Commit();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<UserMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<UserMaster>(@"
                SELECT u.*, r.RoleName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                ORDER BY u.CreatedAt DESC");
        }

        public async Task<UserMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
                SELECT u.*, r.RoleName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                WHERE u.Id = @Id", new { Id = id });
        }

        public async Task<UserMaster?> GetByUsernameAsync(string username)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
                SELECT u.*, r.RoleName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                WHERE u.Username = @Username AND u.IsActive = 1", new { Username = username });
        }

        public async Task<int> CreateAsync(UserMaster user)
        {
            using var conn = CreateConnection();
            var sql = @"
                INSERT INTO UserMaster (Username, Email, PasswordHash, FullName, RoleId, IsActive, CreatedAt)
                VALUES (@Username, @Email, @PasswordHash, @FullName, @RoleId, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            user.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<bool> UpdateAsync(UserMaster user)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE UserMaster SET
                    Email        = @Email,
                    FullName     = @FullName,
                    RoleId       = @RoleId,
                    IsActive     = @IsActive
                WHERE Id = @Id";
            var rows = await conn.ExecuteAsync(sql, user);
            return rows > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int id, string passwordHash)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "UPDATE UserMaster SET PasswordHash = @Hash WHERE Id = @Id",
                new { Hash = passwordHash, Id = id });
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM UserMaster WHERE Id = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "UPDATE UserMaster SET LastLoginDate = @Now WHERE Id = @Id",
                new { Now = DateTime.Now, Id = userId });
            return rows > 0;
        }
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<RoleMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<RoleMaster>(
                "SELECT * FROM RoleMaster ORDER BY RoleName");
        }

        public async Task<RoleMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<RoleMaster>(
                "SELECT * FROM RoleMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(RoleMaster role)
        {
            using var conn = CreateConnection();
            role.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO RoleMaster (RoleName, Description, IsActive, CreatedAt)
                VALUES (@RoleName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, role);
        }

        public async Task<bool> UpdateAsync(RoleMaster role)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE RoleMaster SET
                    RoleName    = @RoleName,
                    Description = @Description,
                    IsActive    = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, role) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM RoleMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

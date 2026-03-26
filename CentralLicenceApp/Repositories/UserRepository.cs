using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
                  SELECT u.*, r.RoleName, l.Name AS LocationName,
                      d.DepartmentName,
                      g.DesignationName,
                                            et.TypeName AS EmployeeTypeName,
                       m.FullName AS ManagerName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                LEFT JOIN LocationMaster l ON u.LocationId = l.Id
                  LEFT JOIN EmployeeDepartmentMaster d ON u.DepartmentId = d.Id
                  LEFT JOIN EmployeeDesignationMaster g ON u.DesignationId = g.Id
                                    LEFT JOIN EmployeeTypeMaster et ON u.EmployeeTypeId = et.Id
                LEFT JOIN UserMaster m ON u.ManagerId = m.Id
                ORDER BY u.CreatedAt DESC");
        }

        public async Task<(IEnumerable<UserMaster> Items, int TotalCount)> GetPagedAsync(string? search, string? status, int? roleId, int page, int pageSize)
        {
            using var conn = CreateConnection();

            const string fromSql = @"
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                LEFT JOIN LocationMaster l ON u.LocationId = l.Id
                LEFT JOIN EmployeeDepartmentMaster d ON u.DepartmentId = d.Id
                LEFT JOIN EmployeeDesignationMaster g ON u.DesignationId = g.Id
                LEFT JOIN EmployeeTypeMaster et ON u.EmployeeTypeId = et.Id
                LEFT JOIN UserMaster m ON u.ManagerId = m.Id";

            var filters = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                filters.Add(@"(
                    u.Username LIKE @Search OR
                    u.FullName LIKE @Search OR
                    u.Email LIKE @Search OR
                    ISNULL(u.PhoneNumber, '') LIKE @Search OR
                    ISNULL(u.EmployeeCode, '') LIKE @Search
                )");
                parameters.Add("Search", $"%{search.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                if (normalizedStatus == "active")
                {
                    filters.Add("u.IsActive = 1");
                }
                else if (normalizedStatus == "inactive")
                {
                    filters.Add("u.IsActive = 0");
                }
            }

            if (roleId.HasValue && roleId.Value > 0)
            {
                filters.Add("u.RoleId = @RoleId");
                parameters.Add("RoleId", roleId.Value);
            }

            var whereSql = filters.Any()
                ? new StringBuilder().Append(" WHERE ").Append(string.Join(" AND ", filters)).ToString()
                : string.Empty;

            var countSql = $"SELECT COUNT(*) {fromSql}{whereSql}";
            var dataSql = $@"
                SELECT u.*, r.RoleName, l.Name AS LocationName,
                       d.DepartmentName,
                       g.DesignationName,
                       et.TypeName AS EmployeeTypeName,
                       m.FullName AS ManagerName
                {fromSql}{whereSql}
                ORDER BY u.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql);
            var items = await conn.QueryAsync<UserMaster>(dataSql, parameters);
            return (items, totalCount);
        }

        public async Task<UserMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
                  SELECT u.*, r.RoleName, l.Name AS LocationName,
                      d.DepartmentName,
                      g.DesignationName,
                                            et.TypeName AS EmployeeTypeName,
                       m.FullName AS ManagerName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                LEFT JOIN LocationMaster l ON u.LocationId = l.Id
                  LEFT JOIN EmployeeDepartmentMaster d ON u.DepartmentId = d.Id
                  LEFT JOIN EmployeeDesignationMaster g ON u.DesignationId = g.Id
                                    LEFT JOIN EmployeeTypeMaster et ON u.EmployeeTypeId = et.Id
                LEFT JOIN UserMaster m ON u.ManagerId = m.Id
                WHERE u.Id = @Id", new { Id = id });
        }

        public async Task<UserMaster?> GetByUsernameAsync(string username)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
                  SELECT u.*, r.RoleName, l.Name AS LocationName,
                      d.DepartmentName,
                      g.DesignationName,
                                            et.TypeName AS EmployeeTypeName,
                       m.FullName AS ManagerName
                FROM UserMaster u
                INNER JOIN RoleMaster r ON u.RoleId = r.Id
                LEFT JOIN LocationMaster l ON u.LocationId = l.Id
                  LEFT JOIN EmployeeDepartmentMaster d ON u.DepartmentId = d.Id
                  LEFT JOIN EmployeeDesignationMaster g ON u.DesignationId = g.Id
                                    LEFT JOIN EmployeeTypeMaster et ON u.EmployeeTypeId = et.Id
                LEFT JOIN UserMaster m ON u.ManagerId = m.Id
                WHERE u.Username = @Username AND u.IsActive = 1", new { Username = username });
        }

        public async Task<int> CreateAsync(UserMaster user)
        {
            using var conn = CreateConnection();
            var sql = @"
                INSERT INTO UserMaster
                    (Username, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth, DateOfJoining, RoleId,
                     LocationId, DepartmentId, DesignationId, EmployeeTypeId, IsEmployee, EmployeeCode, IsCoreMember, ManagerId, ProfileImagePath, IsActive, CreatedAt)
                VALUES
                    (@Username, @Email, @PasswordHash, @FullName, @PhoneNumber, @DateOfBirth, @DateOfJoining, @RoleId,
                     @LocationId, @DepartmentId, @DesignationId, @EmployeeTypeId, @IsEmployee, @EmployeeCode, @IsCoreMember, @ManagerId, @ProfileImagePath, @IsActive, @CreatedAt);
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
                    PhoneNumber  = @PhoneNumber,
                    DateOfBirth  = @DateOfBirth,
                    DateOfJoining = @DateOfJoining,
                    RoleId       = @RoleId,
                    LocationId   = @LocationId,
                    DepartmentId = @DepartmentId,
                    DesignationId = @DesignationId,
                    EmployeeTypeId = @EmployeeTypeId,
                    IsEmployee   = @IsEmployee,
                    EmployeeCode = @EmployeeCode,
                    IsCoreMember = @IsCoreMember,
                    ManagerId    = @ManagerId,
                    ProfileImagePath = @ProfileImagePath,
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

        public async Task<bool> CheckEmployeeCodeUniqueAsync(string employeeCode, int? excludeUserId = null)
        {
            using var conn = CreateConnection();
            var sql = excludeUserId.HasValue
                ? "SELECT COUNT(1) FROM UserMaster WHERE EmployeeCode = @Code AND Id <> @ExcludeId"
                : "SELECT COUNT(1) FROM UserMaster WHERE EmployeeCode = @Code";
            var count = await conn.ExecuteScalarAsync<int>(sql,
                new { Code = employeeCode, ExcludeId = excludeUserId ?? 0 });
            return count == 0;
        }

        public async Task<IEnumerable<UserMaster>> GetEmployeesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<UserMaster>(
                "SELECT Id, FullName, Username FROM UserMaster WHERE IsEmployee = 1 AND IsActive = 1 ORDER BY FullName");
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

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
            var users = (await conn.QueryAsync<UserMaster>(@"
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
                ORDER BY u.CreatedAt DESC")).ToList();
            await PopulateRolesAsync(conn, users);
            return users;
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
                filters.Add("EXISTS (SELECT 1 FROM UserRoleMap ur WHERE ur.UserId = u.Id AND ur.RoleId = @RoleId)");
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
            var items = (await conn.QueryAsync<UserMaster>(dataSql, parameters)).ToList();
            await PopulateRolesAsync(conn, items);
            return (items, totalCount);
        }

        public async Task<UserMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
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
            if (user != null)
            {
                await PopulateRolesAsync(conn, new[] { user });
            }

            return user;
        }

        public async Task<UserMaster?> GetByUsernameAsync(string username)
        {
            using var conn = CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
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
            if (user != null)
            {
                await PopulateRolesAsync(conn, new[] { user });
            }

            return user;
        }

        public async Task<UserMaster?> GetByEmailAsync(string email)
        {
            using var conn = CreateConnection();
            var user = await conn.QuerySingleOrDefaultAsync<UserMaster>(@"
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
                WHERE u.Email = @Email AND u.IsActive = 1", new { Email = email });
            if (user != null)
            {
                await PopulateRolesAsync(conn, new[] { user });
            }

            return user;
        }

        public async Task<int> CreateAsync(UserMaster user)
        {
            using var conn = CreateConnection();
            var roleIds = NormalizeRoleIds(user);
            user.RoleId = roleIds.First();
            var sql = @"
                INSERT INTO UserMaster
                    (Username, Email, PasswordHash, FullName, PhoneNumber, DateOfBirth, DateOfJoining, RoleId,
                     LocationId, DepartmentId, DesignationId, EmployeeTypeId, IsEmployee, EmployeeCode, IsCoreMember, ManagerId, ProfileImagePath, DigitalSignaturePath, IsActive, CreatedAt)
                VALUES
                    (@Username, @Email, @PasswordHash, @FullName, @PhoneNumber, @DateOfBirth, @DateOfJoining, @RoleId,
                     @LocationId, @DepartmentId, @DesignationId, @EmployeeTypeId, @IsEmployee, @EmployeeCode, @IsCoreMember, @ManagerId, @ProfileImagePath, @DigitalSignaturePath, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            user.CreatedAt = DateTime.Now;
            conn.Open();
            using var tx = conn.BeginTransaction();
            var newUserId = await conn.ExecuteScalarAsync<int>(sql, user, tx);
            await SyncUserRolesAsync(conn, tx, newUserId, roleIds);
            tx.Commit();
            return newUserId;
        }

        public async Task<bool> UpdateAsync(UserMaster user)
        {
            using var conn = CreateConnection();
            var roleIds = NormalizeRoleIds(user);
            user.RoleId = roleIds.First();
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
                    DigitalSignaturePath = @DigitalSignaturePath,
                    IsActive     = @IsActive
                WHERE Id = @Id";
            conn.Open();
            using var tx = conn.BeginTransaction();
            var rows = await conn.ExecuteAsync(sql, user, tx);
            await SyncUserRolesAsync(conn, tx, user.Id, roleIds);
            tx.Commit();
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

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var summary = await conn.QuerySingleAsync<UserDeleteReferenceSummary>(@"
                SELECT
                    (SELECT COUNT(1) FROM ExpenseRequest WHERE EmployeeId = @Id) AS SubmittedRequestCount,
                    (SELECT COUNT(1) FROM ExpenseRequest WHERE ApproverId = @Id) AS PendingApprovalCount,
                    (SELECT COUNT(1) FROM ExpenseRequest WHERE ApprovedById = @Id) AS ApprovalActionCount,
                    (SELECT COUNT(1) FROM ExpenseRequest WHERE ReimbursementStartedById = @Id) AS ReimbursementActionCount,
                    (SELECT COUNT(1) FROM ExpenseRequest WHERE SettledById = @Id) AS SettlementActionCount,
                    (SELECT COUNT(1) FROM ExpenseRequestApprovalHistory WHERE ActionByUserId = @Id) AS ApprovalHistoryCount",
                new { Id = id });

            var blockers = new List<string>();

            if (summary.SubmittedRequestCount > 0)
            {
                blockers.Add($"{summary.SubmittedRequestCount} expense request(s) submitted by this user");
            }

            if (summary.PendingApprovalCount > 0)
            {
                blockers.Add($"{summary.PendingApprovalCount} request(s) where this user is assigned as approver");
            }

            var approvalWorkflowCount = summary.ApprovalActionCount + summary.ApprovalHistoryCount;
            if (approvalWorkflowCount > 0)
            {
                blockers.Add($"{approvalWorkflowCount} approval workflow record(s) performed by this user");
            }

            var financeWorkflowCount = summary.ReimbursementActionCount + summary.SettlementActionCount;
            if (financeWorkflowCount > 0)
            {
                blockers.Add($"{financeWorkflowCount} finance processing record(s) completed by this user");
            }

            if (!blockers.Any())
            {
                return (true, null);
            }

            var reason = "This user cannot be deleted because they are linked to "
                + string.Join(", ", blockers)
                + ". Reassign or retain those records first.";

            return (false, reason);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("DELETE FROM UserRoleMap WHERE UserId = @Id", new { Id = id });
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

        public async Task<IEnumerable<UserMaster>> GetCoreMembersAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<UserMaster>(@"
                SELECT Id, Username, Email, FullName, EmployeeCode, IsEmployee, IsCoreMember, IsActive
                FROM UserMaster
                WHERE IsCoreMember = 1 AND IsActive = 1 AND ISNULL(Email, '') <> ''
                ORDER BY ISNULL(FullName, Username)");
        }

        public async Task<IEnumerable<UserMaster>> GetSignatoryUsersAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<UserMaster>(@"
                SELECT Id, Username, FullName, DigitalSignaturePath
                FROM UserMaster
                WHERE IsCoreMember = 1 AND IsActive = 1
                  AND DigitalSignaturePath IS NOT NULL AND DigitalSignaturePath <> ''
                ORDER BY ISNULL(FullName, Username)");
        }

        public async Task<IReadOnlyCollection<int>> GetSelfAndSubordinateIdsAsync(int userId)
        {
            var users = (await GetAllAsync()).ToList();
            var visibleUserIds = new HashSet<int> { userId };
            var pendingManagerIds = new Queue<int>();
            pendingManagerIds.Enqueue(userId);

            while (pendingManagerIds.Count > 0)
            {
                var managerId = pendingManagerIds.Dequeue();
                var directReports = users.Where(user => user.ManagerId == managerId);
                foreach (var directReport in directReports)
                {
                    if (visibleUserIds.Add(directReport.Id))
                    {
                        pendingManagerIds.Enqueue(directReport.Id);
                    }
                }
            }

            return visibleUserIds.ToArray();
        }

        private static List<int> NormalizeRoleIds(UserMaster user)
        {
            var roleIds = user.AssignedRoleIds
                .Where(roleId => roleId > 0)
                .Distinct()
                .ToList();

            if (!roleIds.Any() && user.RoleId > 0)
            {
                roleIds.Add(user.RoleId);
            }

            if (!roleIds.Any())
            {
                throw new InvalidOperationException("At least one role is required for a user.");
            }

            return roleIds;
        }

        private static async Task SyncUserRolesAsync(IDbConnection conn, IDbTransaction tx, int userId, IReadOnlyCollection<int> roleIds)
        {
            await conn.ExecuteAsync("DELETE FROM UserRoleMap WHERE UserId = @UserId", new { UserId = userId }, tx);
            foreach (var roleId in roleIds)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO UserRoleMap(UserId, RoleId, CreatedAt) VALUES(@UserId, @RoleId, GETDATE())",
                    new { UserId = userId, RoleId = roleId },
                    tx);
            }
        }

        private static async Task PopulateRolesAsync(IDbConnection conn, IEnumerable<UserMaster> users)
        {
            var userList = users.ToList();
            if (!userList.Any())
            {
                return;
            }

            var roleRows = (await conn.QueryAsync<UserRoleRow>(@"
                SELECT ur.UserId,
                       r.Id,
                       r.RoleName,
                       r.Description,
                       r.IsActive,
                       r.CreatedAt
                FROM UserRoleMap ur
                INNER JOIN RoleMaster r ON ur.RoleId = r.Id
                WHERE ur.UserId IN @UserIds
                ORDER BY r.RoleName",
                new { UserIds = userList.Select(user => user.Id).Distinct().ToArray() }))
                .ToList();

            var rolesByUser = roleRows
                .GroupBy(row => row.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(row => new RoleMaster
                        {
                            Id = row.Id,
                            RoleName = row.RoleName,
                            Description = row.Description,
                            IsActive = row.IsActive,
                            CreatedAt = row.CreatedAt
                        })
                        .ToList());

            foreach (var user in userList)
            {
                if (!rolesByUser.TryGetValue(user.Id, out var roles))
                {
                    roles = new List<RoleMaster>();
                }

                user.Roles = roles;
                user.AssignedRoleIds = roles.Select(role => role.Id).ToList();
                user.RoleNamesDisplay = roles.Any()
                    ? string.Join(", ", roles.Select(role => role.RoleName))
                    : user.RoleName;

                var primaryRole = roles.FirstOrDefault(role => role.Id == user.RoleId) ?? roles.FirstOrDefault();
                if (primaryRole != null)
                {
                    user.RoleId = primaryRole.Id;
                    user.RoleName = primaryRole.RoleName;
                }
            }
        }

        private sealed class UserRoleRow
        {
            public int UserId { get; set; }
            public int Id { get; set; }
            public string RoleName { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private sealed class UserDeleteReferenceSummary
        {
            public int SubmittedRequestCount { get; init; }
            public int PendingApprovalCount { get; init; }
            public int ApprovalActionCount { get; init; }
            public int ReimbursementActionCount { get; init; }
            public int SettlementActionCount { get; init; }
            public int ApprovalHistoryCount { get; init; }
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

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var summary = await conn.QuerySingleAsync<RoleDeleteReferenceSummary>(@"
                SELECT
                    (SELECT COUNT(1) FROM UserMaster WHERE RoleId = @Id) AS PrimaryUserCount,
                    (SELECT COUNT(1) FROM UserRoleMap WHERE RoleId = @Id) AS RoleAssignmentCount",
                new { Id = id });

            if (summary.PrimaryUserCount == 0 && summary.RoleAssignmentCount == 0)
            {
                return (true, null);
            }

            var blockers = new List<string>();
            if (summary.PrimaryUserCount > 0)
            {
                blockers.Add($"{summary.PrimaryUserCount} user account(s) with this as the primary role");
            }

            if (summary.RoleAssignmentCount > 0)
            {
                blockers.Add($"{summary.RoleAssignmentCount} multi-role assignment(s)");
            }

            return (false, "This role cannot be deleted because it is linked to " + string.Join(", ", blockers) + ". Remove those role assignments first.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM RoleMaster WHERE Id = @Id", new { Id = id }) > 0;
        }

        private sealed class RoleDeleteReferenceSummary
        {
            public int PrimaryUserCount { get; init; }
            public int RoleAssignmentCount { get; init; }
        }
    }
}

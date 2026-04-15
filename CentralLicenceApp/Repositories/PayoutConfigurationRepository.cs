using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public class PayoutConfigurationRepository : IPayoutConfigurationRepository
    {
        private readonly string _connStr;
        public PayoutConfigurationRepository(string connStr) => _connStr = connStr;

        public async Task<IEnumerable<PayoutConfigUserRow>> GetAllConfigurationsAsync(
            string? search, string? modelFilter, string? statusFilter)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT u.Id AS UserId, u.FullName AS UserName, u.EmployeeCode,
                       dept.DepartmentName, desg.DesignationName,
                       CASE WHEN pc.Id IS NOT NULL THEN 1 ELSE 0 END AS IsConfigured,
                       pc.PayoutModel, pc.HourlyRate, pc.DefaultCommissionAmount, pc.EffectiveFrom,
                       ISNULL(rc.RuleCount, 0) AS CommissionRuleCount
                FROM UserMaster u
                LEFT JOIN EmployeeDepartmentMaster dept ON dept.Id = u.DepartmentId
                LEFT JOIN EmployeeDesignationMaster desg ON desg.Id = u.DesignationId
                LEFT JOIN PayoutConfiguration pc ON pc.UserId = u.Id AND pc.IsActive = 1
                OUTER APPLY (
                    SELECT COUNT(*) AS RuleCount FROM PayoutCommissionRule
                    WHERE UserId = u.Id AND IsActive = 1
                ) rc
                WHERE u.IsActive = 1 AND u.IsEmployee = 1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (u.FullName LIKE @Search OR u.EmployeeCode LIKE @Search)";
                parameters.Add("Search", $"%{search.Trim()}%");
            }
            if (!string.IsNullOrWhiteSpace(modelFilter))
            {
                sql += " AND pc.PayoutModel = @Model";
                parameters.Add("Model", modelFilter);
            }
            if (statusFilter == "Configured")
                sql += " AND pc.Id IS NOT NULL";
            else if (statusFilter == "NotConfigured")
                sql += " AND pc.Id IS NULL";

            sql += " ORDER BY u.FullName";
            return await conn.QueryAsync<PayoutConfigUserRow>(sql, parameters);
        }

        public async Task<PayoutConfiguration?> GetConfigurationByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<PayoutConfiguration>(@"
                SELECT pc.*, u.FullName AS UserName, u.EmployeeCode,
                       dept.DepartmentName, desg.DesignationName
                FROM PayoutConfiguration pc
                INNER JOIN UserMaster u ON u.Id = pc.UserId
                LEFT JOIN EmployeeDepartmentMaster dept ON dept.Id = u.DepartmentId
                LEFT JOIN EmployeeDesignationMaster desg ON desg.Id = u.DesignationId
                WHERE pc.UserId = @UserId AND pc.IsActive = 1",
                new { UserId = userId });
        }

        public async Task<bool> UpsertConfigurationAsync(PayoutConfiguration config)
        {
            using var conn = new SqlConnection(_connStr);
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT Id FROM PayoutConfiguration WHERE UserId = @UserId AND IsActive = 1",
                new { config.UserId });

            if (existing.HasValue)
            {
                return await conn.ExecuteAsync(@"
                    UPDATE PayoutConfiguration SET
                        PayoutModel = @PayoutModel,
                        HourlyRate = @HourlyRate,
                        DefaultCommissionAmount = @DefaultCommissionAmount,
                        EffectiveFrom = @EffectiveFrom,
                        UpdatedAt = GETDATE()
                    WHERE Id = @Id",
                    new
                    {
                        Id = existing.Value,
                        config.PayoutModel,
                        HourlyRate = config.PayoutModel == "Hourly" ? config.HourlyRate : null,
                        DefaultCommissionAmount = config.PayoutModel == "Commission" ? config.DefaultCommissionAmount : null,
                        config.EffectiveFrom
                    }) > 0;
            }
            else
            {
                return await conn.ExecuteAsync(@"
                    INSERT INTO PayoutConfiguration (UserId, PayoutModel, HourlyRate, DefaultCommissionAmount, EffectiveFrom, IsActive, CreatedById, CreatedAt)
                    VALUES (@UserId, @PayoutModel, @HourlyRate, @DefaultCommissionAmount, @EffectiveFrom, 1, @CreatedById, GETDATE())",
                    new
                    {
                        config.UserId,
                        config.PayoutModel,
                        HourlyRate = config.PayoutModel == "Hourly" ? config.HourlyRate : null,
                        DefaultCommissionAmount = config.PayoutModel == "Commission" ? config.DefaultCommissionAmount : null,
                        config.EffectiveFrom,
                        config.CreatedById
                    }) > 0;
            }
        }

        // ── Commission Rules ───────────────────────────────────────
        public async Task<IEnumerable<PayoutCommissionRule>> GetCommissionRulesAsync(int userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutCommissionRule>(@"
                SELECT r.*, tt.Name AS TaskTypeName, tc.Name AS TaskCategoryName, pm.Name AS ProjectModuleName
                FROM PayoutCommissionRule r
                LEFT JOIN TaskTypeMaster tt ON tt.Id = r.TaskTypeId
                LEFT JOIN TaskCategoryMaster tc ON tc.Id = r.TaskCategoryId
                LEFT JOIN ProjectModuleMaster pm ON pm.Id = r.ProjectModuleId
                WHERE r.UserId = @UserId AND r.IsActive = 1
                ORDER BY r.Priority DESC, r.Amount DESC",
                new { UserId = userId });
        }

        public async Task<PayoutCommissionRule?> GetCommissionRuleByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<PayoutCommissionRule>(@"
                SELECT r.*, tt.Name AS TaskTypeName, tc.Name AS TaskCategoryName, pm.Name AS ProjectModuleName
                FROM PayoutCommissionRule r
                LEFT JOIN TaskTypeMaster tt ON tt.Id = r.TaskTypeId
                LEFT JOIN TaskCategoryMaster tc ON tc.Id = r.TaskCategoryId
                LEFT JOIN ProjectModuleMaster pm ON pm.Id = r.ProjectModuleId
                WHERE r.Id = @Id",
                new { Id = id });
        }

        public async Task<int> AddCommissionRuleAsync(PayoutCommissionRule rule)
        {
            rule.Priority = ComputePriority(rule);
            using var conn = new SqlConnection(_connStr);
            return await conn.QuerySingleAsync<int>(@"
                INSERT INTO PayoutCommissionRule
                    (UserId, TaskTypeId, TaskCategoryId, ProjectModuleId, Amount, Priority, EffectiveFrom, IsActive, CreatedById, CreatedAt)
                VALUES
                    (@UserId, @TaskTypeId, @TaskCategoryId, @ProjectModuleId, @Amount, @Priority, @EffectiveFrom, 1, @CreatedById, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new
                {
                    rule.UserId,
                    rule.TaskTypeId,
                    rule.TaskCategoryId,
                    rule.ProjectModuleId,
                    rule.Amount,
                    rule.Priority,
                    rule.EffectiveFrom,
                    rule.CreatedById
                });
        }

        public async Task<bool> UpdateCommissionRuleAsync(PayoutCommissionRule rule)
        {
            rule.Priority = ComputePriority(rule);
            using var conn = new SqlConnection(_connStr);
            return await conn.ExecuteAsync(@"
                UPDATE PayoutCommissionRule SET
                    TaskTypeId = @TaskTypeId,
                    TaskCategoryId = @TaskCategoryId,
                    ProjectModuleId = @ProjectModuleId,
                    Amount = @Amount,
                    Priority = @Priority,
                    EffectiveFrom = @EffectiveFrom
                WHERE Id = @Id",
                new
                {
                    rule.Id,
                    rule.TaskTypeId,
                    rule.TaskCategoryId,
                    rule.ProjectModuleId,
                    rule.Amount,
                    rule.Priority,
                    rule.EffectiveFrom
                }) > 0;
        }

        public async Task<bool> DeleteCommissionRuleAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.ExecuteAsync(
                "UPDATE PayoutCommissionRule SET IsActive = 0 WHERE Id = @Id",
                new { Id = id }) > 0;
        }

        // ── Priority Computation ───────────────────────────────────
        private static int ComputePriority(PayoutCommissionRule rule) =>
            rule.ProjectModuleId.HasValue ? 30
            : rule.TaskCategoryId.HasValue ? 20
            : rule.TaskTypeId.HasValue ? 10
            : 0;
    }
}

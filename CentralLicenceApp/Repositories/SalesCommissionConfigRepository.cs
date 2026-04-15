using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public class SalesCommissionConfigRepository : ISalesCommissionConfigRepository
    {
        private readonly string _connStr;
        public SalesCommissionConfigRepository(string connStr) => _connStr = connStr;

        public async Task<IEnumerable<SalesCommConfigUserRow>> GetAllConfigurationsAsync(
            string? search, string? typeFilter, string? statusFilter)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT u.Id AS UserId, u.FullName AS UserName, u.EmployeeCode,
                       dept.DepartmentName, desg.DesignationName,
                       CASE WHEN sc.Id IS NOT NULL THEN 1 ELSE 0 END AS IsConfigured,
                       sc.CommissionType, sc.DefaultRate, sc.EffectiveFrom,
                       ISNULL(rc.RuleCount, 0) AS RuleCount
                FROM UserMaster u
                LEFT JOIN EmployeeDepartmentMaster dept ON dept.Id = u.DepartmentId
                LEFT JOIN EmployeeDesignationMaster desg ON desg.Id = u.DesignationId
                LEFT JOIN SalesCommissionConfiguration sc ON sc.UserId = u.Id AND sc.IsActive = 1
                OUTER APPLY (
                    SELECT COUNT(*) AS RuleCount FROM SalesCommissionRule
                    WHERE UserId = u.Id AND IsActive = 1
                ) rc
                WHERE u.IsActive = 1 AND u.IsSalesAgent = 1";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (u.FullName LIKE @Search OR u.EmployeeCode LIKE @Search)";
                parameters.Add("Search", $"%{search.Trim()}%");
            }
            if (!string.IsNullOrWhiteSpace(typeFilter))
            {
                sql += " AND sc.CommissionType = @Type";
                parameters.Add("Type", typeFilter);
            }
            if (statusFilter == "Configured")
                sql += " AND sc.Id IS NOT NULL";
            else if (statusFilter == "NotConfigured")
                sql += " AND sc.Id IS NULL";

            sql += " ORDER BY u.FullName";
            return await conn.QueryAsync<SalesCommConfigUserRow>(sql, parameters);
        }

        public async Task<SalesCommissionConfiguration?> GetConfigurationByUserIdAsync(int userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<SalesCommissionConfiguration>(@"
                SELECT sc.*, u.FullName AS UserName, u.EmployeeCode,
                       dept.DepartmentName, desg.DesignationName
                FROM SalesCommissionConfiguration sc
                INNER JOIN UserMaster u ON u.Id = sc.UserId
                LEFT JOIN EmployeeDepartmentMaster dept ON dept.Id = u.DepartmentId
                LEFT JOIN EmployeeDesignationMaster desg ON desg.Id = u.DesignationId
                WHERE sc.UserId = @UserId AND sc.IsActive = 1",
                new { UserId = userId });
        }

        public async Task<bool> UpsertConfigurationAsync(SalesCommissionConfiguration config)
        {
            using var conn = new SqlConnection(_connStr);
            var existing = await conn.QueryFirstOrDefaultAsync<int?>(
                "SELECT Id FROM SalesCommissionConfiguration WHERE UserId = @UserId AND IsActive = 1",
                new { config.UserId });

            if (existing.HasValue)
            {
                return await conn.ExecuteAsync(@"
                    UPDATE SalesCommissionConfiguration SET
                        CommissionType = @CommissionType,
                        DefaultRate = @DefaultRate,
                        EffectiveFrom = @EffectiveFrom,
                        UpdatedAt = GETDATE()
                    WHERE Id = @Id",
                    new
                    {
                        Id = existing.Value,
                        config.CommissionType,
                        config.DefaultRate,
                        config.EffectiveFrom
                    }) > 0;
            }
            else
            {
                return await conn.ExecuteAsync(@"
                    INSERT INTO SalesCommissionConfiguration
                        (UserId, CommissionType, DefaultRate, EffectiveFrom, IsActive, CreatedById, CreatedAt)
                    VALUES
                        (@UserId, @CommissionType, @DefaultRate, @EffectiveFrom, 1, @CreatedById, GETDATE())",
                    new
                    {
                        config.UserId,
                        config.CommissionType,
                        config.DefaultRate,
                        config.EffectiveFrom,
                        config.CreatedById
                    }) > 0;
            }
        }

        // ── Commission Rules ───────────────────────────────────────

        public async Task<IEnumerable<SalesCommissionRule>> GetRulesAsync(int userId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommissionRule>(@"
                SELECT r.*, p.ProductName AS ProductName
                FROM SalesCommissionRule r
                LEFT JOIN ProductMaster p ON p.Id = r.ProductId
                WHERE r.UserId = @UserId AND r.IsActive = 1
                ORDER BY r.Priority DESC, r.Rate DESC",
                new { UserId = userId });
        }

        public async Task<SalesCommissionRule?> GetRuleByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<SalesCommissionRule>(@"
                SELECT r.*, p.ProductName AS ProductName
                FROM SalesCommissionRule r
                LEFT JOIN ProductMaster p ON p.Id = r.ProductId
                WHERE r.Id = @Id",
                new { Id = id });
        }

        public async Task<int> AddRuleAsync(SalesCommissionRule rule)
        {
            rule.Priority = ComputePriority(rule);
            using var conn = new SqlConnection(_connStr);
            return await conn.QuerySingleAsync<int>(@"
                INSERT INTO SalesCommissionRule
                    (UserId, ProductId, CommissionType, Rate, Priority, EffectiveFrom, IsActive, CreatedById, CreatedAt)
                VALUES
                    (@UserId, @ProductId, @CommissionType, @Rate, @Priority, @EffectiveFrom, 1, @CreatedById, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new
                {
                    rule.UserId,
                    rule.ProductId,
                    rule.CommissionType,
                    rule.Rate,
                    rule.Priority,
                    rule.EffectiveFrom,
                    rule.CreatedById
                });
        }

        public async Task<bool> UpdateRuleAsync(SalesCommissionRule rule)
        {
            rule.Priority = ComputePriority(rule);
            using var conn = new SqlConnection(_connStr);
            return await conn.ExecuteAsync(@"
                UPDATE SalesCommissionRule SET
                    ProductId = @ProductId,
                    CommissionType = @CommissionType,
                    Rate = @Rate,
                    Priority = @Priority,
                    EffectiveFrom = @EffectiveFrom
                WHERE Id = @Id",
                new
                {
                    rule.Id,
                    rule.ProductId,
                    rule.CommissionType,
                    rule.Rate,
                    rule.Priority,
                    rule.EffectiveFrom
                }) > 0;
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.ExecuteAsync(
                "UPDATE SalesCommissionRule SET IsActive = 0 WHERE Id = @Id",
                new { Id = id }) > 0;
        }

        private static int ComputePriority(SalesCommissionRule rule) =>
            rule.ProductId.HasValue ? 10 : 0;
    }
}

using System.Data;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PayoutBatchRepository : IPayoutBatchRepository
    {
        private readonly string _connStr;
        public PayoutBatchRepository(string connStr) => _connStr = connStr;

        // ── Batch Listing ──────────────────────────────────────────
        public async Task<IEnumerable<PayoutBatch>> GetBatchesAsync(int? userId, string? statusFilter, DateTime? fromDate, DateTime? toDate)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName
                FROM PayoutBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                WHERE 1 = 1";

            var p = new DynamicParameters();
            if (userId.HasValue) { sql += " AND b.UserId = @UserId"; p.Add("UserId", userId.Value); }
            if (!string.IsNullOrEmpty(statusFilter)) { sql += " AND b.Status = @Status"; p.Add("Status", statusFilter); }
            if (fromDate.HasValue) { sql += " AND b.ToDate >= @FromDate"; p.Add("FromDate", fromDate.Value); }
            if (toDate.HasValue) { sql += " AND b.FromDate <= @ToDate"; p.Add("ToDate", toDate.Value); }

            sql += " ORDER BY b.GeneratedAt DESC";
            return await conn.QueryAsync<PayoutBatch>(sql, p);
        }

        // ── Single Batch ───────────────────────────────────────────
        public async Task<PayoutBatch?> GetBatchByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<PayoutBatch>(@"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName,
                       s.FullName AS SettledByName,
                       bk.BankName
                FROM PayoutBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                LEFT JOIN UserMaster s ON s.Id = b.SettledById
                LEFT JOIN BankMaster bk ON bk.Id = b.SettlementBankId
                WHERE b.Id = @Id", new { Id = id });
        }

        // ── Batch Lines ────────────────────────────────────────────
        public async Task<IEnumerable<PayoutBatchLine>> GetBatchLinesAsync(int batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutBatchLine>(@"
                SELECT * FROM PayoutBatchLine
                WHERE BatchId = @BatchId
                ORDER BY TaskCompletedAt, TaskTitle", new { BatchId = batchId });
        }

        // ── Approval History ───────────────────────────────────────
        public async Task<IEnumerable<PayoutApprovalHistory>> GetApprovalHistoryAsync(int batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<PayoutApprovalHistory>(@"
                SELECT h.*, u.FullName AS ApprovedByName
                FROM PayoutApprovalHistory h
                INNER JOIN UserMaster u ON u.Id = h.ApprovedById
                WHERE h.BatchId = @BatchId
                ORDER BY h.ApprovedAt", new { BatchId = batchId });
        }

        // ── Delete Draft ───────────────────────────────────────────
        public async Task<bool> DeleteBatchAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            // Only Draft batches can be deleted; lines cascade
            var rows = await conn.ExecuteAsync(@"
                DELETE FROM PayoutBatch WHERE Id = @Id AND Status = 'Draft'", new { Id = id });
            return rows > 0;
        }

        // ── Preview Eligible Tasks ─────────────────────────────────
        public async Task<PayoutPreviewResult?> PreviewEligibleTasksAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            using var conn = new SqlConnection(_connStr);

            // Get config
            var config = await conn.QueryFirstOrDefaultAsync<PayoutConfiguration>(@"
                SELECT * FROM PayoutConfiguration
                WHERE UserId = @UserId AND IsActive = 1", new { UserId = userId });
            if (config == null) return null;

            // Find eligible tasks: completed, assigned to user, in date range, not already in a batch line
            var tasks = (await conn.QueryAsync<EligibleTask>(EligibleTasksSql, new { UserId = userId, FromDate = fromDate, ToDate = toDate })).ToList();

            if (!tasks.Any())
                return new PayoutPreviewResult { PayoutModel = config.PayoutModel, FormattedTime = "0m" };

            decimal total = 0;
            int totalMinutes = tasks.Sum(t => t.TimeSpentMinutes);

            if (config.PayoutModel == "Hourly")
            {
                total = (totalMinutes / 60m) * (config.HourlyRate ?? 0);
            }
            else
            {
                // Load commission rules
                var rules = (await conn.QueryAsync<PayoutCommissionRule>(@"
                    SELECT * FROM PayoutCommissionRule
                    WHERE UserId = @UserId AND IsActive = 1 AND EffectiveFrom <= @ToDate
                    ORDER BY Priority DESC", new { UserId = userId, ToDate = toDate })).ToList();

                foreach (var t in tasks)
                {
                    total += ResolveCommissionAmount(t, rules, config.DefaultCommissionAmount ?? 0);
                }
            }

            var h = totalMinutes / 60;
            var m = totalMinutes % 60;
            var formatted = h > 0 && m > 0 ? $"{h}h {m}m" : h > 0 ? $"{h}h" : $"{m}m";

            return new PayoutPreviewResult
            {
                EligibleTasks = tasks.Count,
                TotalMinutes = totalMinutes,
                EstimatedAmount = Math.Round(total, 2),
                PayoutModel = config.PayoutModel,
                FormattedTime = formatted
            };
        }

        // ── Generate Batch ─────────────────────────────────────────
        public async Task<int> GenerateBatchAsync(int userId, DateTime fromDate, DateTime toDate, string? remarks, int generatedById)
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Get user's config
                var config = await conn.QueryFirstOrDefaultAsync<PayoutConfiguration>(@"
                    SELECT * FROM PayoutConfiguration
                    WHERE UserId = @UserId AND IsActive = 1",
                    new { UserId = userId }, tx);

                if (config == null)
                    throw new InvalidOperationException("Payout configuration not found for this user.");

                // 2. Get eligible tasks
                var tasks = (await conn.QueryAsync<EligibleTask>(EligibleTasksSql,
                    new { UserId = userId, FromDate = fromDate, ToDate = toDate }, tx)).ToList();

                if (!tasks.Any())
                    throw new InvalidOperationException("No eligible tasks found in the selected date range.");

                // 3. Load commission rules (for Commission model)
                List<PayoutCommissionRule> rules = new();
                if (config.PayoutModel == "Commission")
                {
                    rules = (await conn.QueryAsync<PayoutCommissionRule>(@"
                        SELECT * FROM PayoutCommissionRule
                        WHERE UserId = @UserId AND IsActive = 1 AND EffectiveFrom <= @ToDate
                        ORDER BY Priority DESC",
                        new { UserId = userId, ToDate = toDate }, tx)).ToList();
                }

                // 4. Build lines and compute amounts
                var lines = new List<PayoutBatchLine>();
                decimal grossAmount = 0;
                int totalMinutes = 0;

                foreach (var t in tasks)
                {
                    decimal rate;
                    string rateSource;

                    if (config.PayoutModel == "Hourly")
                    {
                        rate = config.HourlyRate ?? 0;
                        rateSource = "Hourly";
                    }
                    else
                    {
                        (rate, rateSource) = ResolveCommissionRateAndSource(t, rules, config.DefaultCommissionAmount ?? 0);
                    }

                    decimal amount = config.PayoutModel == "Hourly"
                        ? Math.Round((t.TimeSpentMinutes / 60m) * rate, 2)
                        : rate; // Commission is per task

                    grossAmount += amount;
                    totalMinutes += t.TimeSpentMinutes;

                    lines.Add(new PayoutBatchLine
                    {
                        TaskId = t.TaskId,
                        TaskTitle = t.TaskTitle,
                        TaskTypeName = t.TaskTypeName,
                        TaskCategoryName = t.TaskCategoryName,
                        ProjectModuleName = t.ProjectModuleName,
                        TimeSpentMinutes = t.TimeSpentMinutes,
                        RateApplied = rate,
                        RateSource = rateSource,
                        Amount = amount,
                        TaskCompletedAt = t.TaskCompletedAt
                    });
                }

                // 5. Insert batch
                var batchId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO PayoutBatch (UserId, FromDate, ToDate, PayoutModel, HourlyRateSnapshot,
                        TotalMinutes, TotalTasks, GrossAmount, DeductionAmount, NetAmount,
                        Status, Remarks, GeneratedById)
                    VALUES (@UserId, @FromDate, @ToDate, @PayoutModel, @HourlyRateSnapshot,
                        @TotalMinutes, @TotalTasks, @GrossAmount, 0, @GrossAmount,
                        'Draft', @Remarks, @GeneratedById);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new
                    {
                        UserId = userId,
                        FromDate = fromDate,
                        ToDate = toDate,
                        PayoutModel = config.PayoutModel,
                        HourlyRateSnapshot = config.PayoutModel == "Hourly" ? config.HourlyRate : null,
                        TotalMinutes = totalMinutes,
                        TotalTasks = lines.Count,
                        GrossAmount = grossAmount,
                        Remarks = remarks,
                        GeneratedById = generatedById
                    }, tx);

                // 6. Insert lines
                foreach (var line in lines)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO PayoutBatchLine (BatchId, TaskId, TaskTitle, TaskTypeName,
                            TaskCategoryName, ProjectModuleName, TimeSpentMinutes,
                            RateApplied, RateSource, Amount, TaskCompletedAt)
                        VALUES (@BatchId, @TaskId, @TaskTitle, @TaskTypeName,
                            @TaskCategoryName, @ProjectModuleName, @TimeSpentMinutes,
                            @RateApplied, @RateSource, @Amount, @TaskCompletedAt)",
                        new
                        {
                            BatchId = batchId,
                            line.TaskId,
                            line.TaskTitle,
                            line.TaskTypeName,
                            line.TaskCategoryName,
                            line.ProjectModuleName,
                            line.TimeSpentMinutes,
                            line.RateApplied,
                            line.RateSource,
                            line.Amount,
                            line.TaskCompletedAt
                        }, tx);
                }

                tx.Commit();
                return batchId;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ── Submit for Approval ────────────────────────────────────
        public async Task<bool> SubmitForApprovalAsync(int batchId)
        {
            using var conn = new SqlConnection(_connStr);
            var rows = await conn.ExecuteAsync(@"
                UPDATE PayoutBatch SET Status = 'PendingApproval'
                WHERE Id = @Id AND Status = 'Draft'", new { Id = batchId });
            return rows > 0;
        }

        // ── Approve / Reject ───────────────────────────────────────
        public async Task<bool> ApproveOrRejectAsync(int batchId, int approverLevel, int approvedById, string status, string? remarks)
        {
            const int maxRetries = 2;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var conn = new SqlConnection(_connStr);
                    await conn.OpenAsync();
                    using var tx = conn.BeginTransaction();

                    try
                    {
                        // Insert approval record
                        await conn.ExecuteAsync(@"
                            INSERT INTO PayoutApprovalHistory (BatchId, ApproverLevel, ApprovedById, Status, Remarks)
                            VALUES (@BatchId, @Level, @ApprovedById, @Status, @Remarks)",
                            new { BatchId = batchId, Level = approverLevel, ApprovedById = approvedById, Status = status, Remarks = remarks }, tx);

                        // Update batch status
                        string newStatus;
                        if (status == "Rejected")
                        {
                            newStatus = "Rejected";
                        }
                        else if (approverLevel == 1)
                        {
                            newStatus = "L1Approved";
                        }
                        else
                        {
                            newStatus = "Approved";
                        }

                        string requiredCurrentStatus = approverLevel == 1 ? "PendingApproval" : "L1Approved";
                        var rows = await conn.ExecuteAsync(@"
                            UPDATE PayoutBatch SET Status = @NewStatus
                            WHERE Id = @Id AND Status = @CurrentStatus",
                            new { Id = batchId, NewStatus = newStatus, CurrentStatus = requiredCurrentStatus }, tx);

                        if (rows == 0) { tx.Rollback(); return false; }

                        tx.Commit();
                        return true;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
                catch (SqlException ex) when (attempt < maxRetries && (ex.Number == 35 || ex.Number == -2 || ex.Number == 258 || ex.Class == 20))
                {
                    // Transient connection error — retry after brief delay
                    await Task.Delay(500 * (attempt + 1));
                }
            }
            return false;
        }

        // ── Pending Approvals ──────────────────────────────────────
        public async Task<IEnumerable<PayoutBatch>> GetPendingApprovalsAsync(int approverLevel, int? approverUserId)
        {
            using var conn = new SqlConnection(_connStr);
            var requiredStatus = approverLevel == 1 ? "PendingApproval" : "L1Approved";
            return await conn.QueryAsync<PayoutBatch>(@"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName
                FROM PayoutBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                WHERE b.Status = @Status
                ORDER BY b.GeneratedAt DESC",
                new { Status = requiredStatus });
        }

        // ── Settlement ─────────────────────────────────────────────
        public async Task<bool> SettleBatchAsync(PayoutSettlementFormViewModel model, int settledById)
        {
            using var conn = new SqlConnection(_connStr);
            var rows = await conn.ExecuteAsync(@"
                UPDATE PayoutBatch
                SET Status = 'Paid',
                    SettlementAmount = @SettlementAmount,
                    SettlementDate = @SettlementDate,
                    SettledAt = GETDATE(),
                    SettledById = @SettledById,
                    SettlementMode = @SettlementMode,
                    SettlementReferenceNo = @SettlementReferenceNo,
                    SettlementBankId = @SettlementBankId,
                    SettlementRemarks = @SettlementRemarks
                WHERE Id = @BatchId AND Status = 'Approved'",
                new
                {
                    model.BatchId,
                    model.SettlementAmount,
                    model.SettlementDate,
                    SettledById = settledById,
                    model.SettlementMode,
                    model.SettlementReferenceNo,
                    model.SettlementBankId,
                    model.SettlementRemarks
                });
            return rows > 0;
        }

        // ── Private Helpers ────────────────────────────────────────

        private const string EligibleTasksSql = @"
            SELECT
                t.Id AS TaskId,
                t.TaskTitle,
                tt.Name AS TaskTypeName,
                tc.Name AS TaskCategoryName,
                pm.Name AS ProjectModuleName,
                t.TaskTypeId,
                t.TaskCategoryId,
                t.ProjectModuleId,
                ISNULL(tl.TotalMins, t.TimeSpentMinutes) AS TimeSpentMinutes,
                t.UpdatedAt AS TaskCompletedAt
            FROM DailyTaskLog t
            INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
            INNER JOIN TaskCategoryMaster tc ON tc.Id = t.TaskCategoryId
            LEFT JOIN ProjectModuleMaster pm ON pm.Id = t.ProjectModuleId
            OUTER APPLY (
                SELECT SUM(TimeSpentMinutes) AS TotalMins
                FROM TaskTimeLog WHERE TaskId = t.Id AND UserId = @UserId
            ) tl
            WHERE t.AssignedToUserId = @UserId
              AND t.Status = 'Completed'
              AND t.TaskDate BETWEEN @FromDate AND @ToDate
              AND NOT EXISTS (
                  SELECT 1 FROM PayoutBatchLine bl
                  INNER JOIN PayoutBatch pb ON pb.Id = bl.BatchId
                  WHERE bl.TaskId = t.Id AND pb.Status <> 'Rejected'
              )
            ORDER BY t.TaskDate, t.TaskTitle";

        private static decimal ResolveCommissionAmount(EligibleTask task, List<PayoutCommissionRule> rules, decimal defaultAmount)
        {
            var (amount, _) = ResolveCommissionRateAndSource(task, rules, defaultAmount);
            return amount;
        }

        private static (decimal Rate, string Source) ResolveCommissionRateAndSource(EligibleTask task, List<PayoutCommissionRule> rules, decimal defaultAmount)
        {
            // Rules are ordered by Priority DESC — first match wins
            foreach (var r in rules)
            {
                if (r.ProjectModuleId.HasValue && r.ProjectModuleId == task.ProjectModuleId)
                    return (r.Amount, "Project");
                if (r.TaskCategoryId.HasValue && r.TaskCategoryId == task.TaskCategoryId)
                    return (r.Amount, "Category");
                if (r.TaskTypeId.HasValue && r.TaskTypeId == task.TaskTypeId)
                    return (r.Amount, "TaskType");
                if (!r.ProjectModuleId.HasValue && !r.TaskCategoryId.HasValue && !r.TaskTypeId.HasValue)
                    return (r.Amount, "Default");
            }
            return (defaultAmount, "Default");
        }

        // Internal DTO for eligible task query
        private class EligibleTask
        {
            public int TaskId { get; set; }
            public string TaskTitle { get; set; } = string.Empty;
            public string TaskTypeName { get; set; } = string.Empty;
            public string TaskCategoryName { get; set; } = string.Empty;
            public string? ProjectModuleName { get; set; }
            public int TaskTypeId { get; set; }
            public int TaskCategoryId { get; set; }
            public int? ProjectModuleId { get; set; }
            public int TimeSpentMinutes { get; set; }
            public DateTime? TaskCompletedAt { get; set; }
        }
    }
}

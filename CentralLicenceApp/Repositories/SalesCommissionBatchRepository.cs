using System.Data;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class SalesCommissionBatchRepository : ISalesCommissionBatchRepository
    {
        private readonly string _connStr;
        public SalesCommissionBatchRepository(string connStr) => _connStr = connStr;

        // ── Batch Listing ──────────────────────────────────────────
        public async Task<IEnumerable<SalesCommissionBatch>> GetBatchesAsync(
            int? userId, string? statusFilter, DateTime? fromDate, DateTime? toDate)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName
                FROM SalesCommissionBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                WHERE 1 = 1";

            var p = new DynamicParameters();
            if (userId.HasValue) { sql += " AND b.UserId = @UserId"; p.Add("UserId", userId.Value); }
            if (!string.IsNullOrEmpty(statusFilter)) { sql += " AND b.Status = @Status"; p.Add("Status", statusFilter); }
            if (fromDate.HasValue) { sql += " AND b.ToDate >= @FromDate"; p.Add("FromDate", fromDate.Value); }
            if (toDate.HasValue) { sql += " AND b.FromDate <= @ToDate"; p.Add("ToDate", toDate.Value); }

            sql += " ORDER BY b.GeneratedAt DESC";
            return await conn.QueryAsync<SalesCommissionBatch>(sql, p);
        }

        // ── Single Batch ───────────────────────────────────────────
        public async Task<SalesCommissionBatch?> GetBatchByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryFirstOrDefaultAsync<SalesCommissionBatch>(@"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName,
                       s.FullName AS SettledByName,
                       bk.BankName
                FROM SalesCommissionBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                LEFT JOIN UserMaster s ON s.Id = b.SettledById
                LEFT JOIN BankMaster bk ON bk.Id = b.SettlementBankId
                WHERE b.Id = @Id", new { Id = id });
        }

        // ── Batch Lines ────────────────────────────────────────────
        public async Task<IEnumerable<SalesCommissionBatchLine>> GetBatchLinesAsync(int batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommissionBatchLine>(@"
                SELECT * FROM SalesCommissionBatchLine
                WHERE BatchId = @BatchId
                ORDER BY PaymentDate, InvoiceNo", new { BatchId = batchId });
        }

        // ── Approval History ───────────────────────────────────────
        public async Task<IEnumerable<SalesCommissionApprovalHistory>> GetApprovalHistoryAsync(int batchId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesCommissionApprovalHistory>(@"
                SELECT h.*, u.FullName AS ApprovedByName
                FROM SalesCommissionApprovalHistory h
                INNER JOIN UserMaster u ON u.Id = h.ApprovedById
                WHERE h.BatchId = @BatchId
                ORDER BY h.ApprovedAt", new { BatchId = batchId });
        }

        // ── Delete Draft ───────────────────────────────────────────
        public async Task<bool> DeleteBatchAsync(int id)
        {
            using var conn = new SqlConnection(_connStr);
            var rows = await conn.ExecuteAsync(
                "DELETE FROM SalesCommissionBatch WHERE Id = @Id AND Status = 'Draft'", new { Id = id });
            return rows > 0;
        }

        // ── Preview Eligible Payments ──────────────────────────────
        public async Task<SalesCommPreviewResult?> PreviewEligiblePaymentsAsync(
            int userId, DateTime fromDate, DateTime toDate)
        {
            using var conn = new SqlConnection(_connStr);

            var config = await conn.QueryFirstOrDefaultAsync<SalesCommissionConfiguration>(
                "SELECT * FROM SalesCommissionConfiguration WHERE UserId = @UserId AND IsActive = 1",
                new { UserId = userId });
            if (config == null) return null;

            var payments = (await conn.QueryAsync<EligiblePayment>(EligiblePaymentsSql,
                new { SalesUserId = userId, FromDate = fromDate, ToDate = toDate })).ToList();

            if (!payments.Any())
                return new SalesCommPreviewResult { CommissionType = config.CommissionType };

            // Load product rules
            var rules = (await conn.QueryAsync<SalesCommissionRule>(@"
                SELECT * FROM SalesCommissionRule
                WHERE UserId = @UserId AND IsActive = 1 AND EffectiveFrom <= @ToDate
                ORDER BY Priority DESC",
                new { UserId = userId, ToDate = toDate })).ToList();

            decimal totalCommission = 0;
            foreach (var pay in payments)
            {
                totalCommission += ResolveCommission(pay, config, rules);
            }

            return new SalesCommPreviewResult
            {
                EligiblePayments = payments.Count,
                InvoiceCount = payments.Select(p => p.InvoiceId).Distinct().Count(),
                TotalSalesAmount = payments.Sum(p => p.PaymentAmount),
                EstimatedCommission = Math.Round(totalCommission, 2),
                CommissionType = config.CommissionType
            };
        }

        // ── Generate Batch ─────────────────────────────────────────
        public async Task<int> GenerateBatchAsync(
            int userId, DateTime fromDate, DateTime toDate, string? remarks, int generatedById)
        {
            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // 1. Get config
                var config = await conn.QueryFirstOrDefaultAsync<SalesCommissionConfiguration>(
                    "SELECT * FROM SalesCommissionConfiguration WHERE UserId = @UserId AND IsActive = 1",
                    new { UserId = userId }, tx);
                if (config == null)
                    throw new InvalidOperationException("Sales commission configuration not found for this user.");

                // 2. Get eligible payments
                var payments = (await conn.QueryAsync<EligiblePayment>(EligiblePaymentsSql,
                    new { SalesUserId = userId, FromDate = fromDate, ToDate = toDate }, tx)).ToList();
                if (!payments.Any())
                    throw new InvalidOperationException("No eligible payments found in the selected date range.");

                // 3. Load product rules
                var rules = (await conn.QueryAsync<SalesCommissionRule>(@"
                    SELECT * FROM SalesCommissionRule
                    WHERE UserId = @UserId AND IsActive = 1 AND EffectiveFrom <= @ToDate
                    ORDER BY Priority DESC",
                    new { UserId = userId, ToDate = toDate }, tx)).ToList();

                // 4. Build lines
                var lines = new List<SalesCommissionBatchLine>();
                decimal grossCommission = 0;

                foreach (var pay in payments)
                {
                    var (rate, source, commType) = ResolveRateAndSource(pay, config, rules);
                    decimal commission = commType == "Percentage"
                        ? Math.Round(pay.PaymentAmount * rate / 100m, 2)
                        : rate;

                    grossCommission += commission;
                    lines.Add(new SalesCommissionBatchLine
                    {
                        InvoicePaymentId = pay.InvoicePaymentId,
                        InvoiceId = pay.InvoiceId,
                        InvoiceNo = pay.InvoiceNo,
                        PartyName = pay.PartyName,
                        PaymentDate = pay.PaymentDate,
                        PaymentAmount = pay.PaymentAmount,
                        ProductId = pay.ProductId,
                        ProductName = pay.ProductName,
                        CommissionType = commType,
                        RateApplied = rate,
                        RateSource = source,
                        CommissionAmount = commission
                    });
                }

                // 5. Insert batch
                var batchId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO SalesCommissionBatch
                        (UserId, FromDate, ToDate, CommissionTypeSnapshot, DefaultRateSnapshot,
                         TotalInvoices, TotalPayments, TotalSalesAmount,
                         GrossCommission, DeductionAmount, NetCommission,
                         Status, Remarks, GeneratedById)
                    VALUES
                        (@UserId, @FromDate, @ToDate, @CommissionType, @DefaultRate,
                         @TotalInvoices, @TotalPayments, @TotalSalesAmount,
                         @GrossCommission, 0, @GrossCommission,
                         'Draft', @Remarks, @GeneratedById);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new
                    {
                        UserId = userId,
                        FromDate = fromDate,
                        ToDate = toDate,
                        CommissionType = config.CommissionType,
                        DefaultRate = config.DefaultRate,
                        TotalInvoices = lines.Select(l => l.InvoiceId).Distinct().Count(),
                        TotalPayments = lines.Count,
                        TotalSalesAmount = lines.Sum(l => l.PaymentAmount),
                        GrossCommission = grossCommission,
                        Remarks = remarks,
                        GeneratedById = generatedById
                    }, tx);

                // 6. Insert lines
                foreach (var line in lines)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO SalesCommissionBatchLine
                            (BatchId, InvoicePaymentId, InvoiceId, InvoiceNo, PartyName,
                             PaymentDate, PaymentAmount, ProductId, ProductName,
                             CommissionType, RateApplied, RateSource, CommissionAmount)
                        VALUES
                            (@BatchId, @InvoicePaymentId, @InvoiceId, @InvoiceNo, @PartyName,
                             @PaymentDate, @PaymentAmount, @ProductId, @ProductName,
                             @CommissionType, @RateApplied, @RateSource, @CommissionAmount)",
                        new
                        {
                            BatchId = batchId,
                            line.InvoicePaymentId,
                            line.InvoiceId,
                            line.InvoiceNo,
                            line.PartyName,
                            line.PaymentDate,
                            line.PaymentAmount,
                            line.ProductId,
                            line.ProductName,
                            line.CommissionType,
                            line.RateApplied,
                            line.RateSource,
                            line.CommissionAmount
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
            var rows = await conn.ExecuteAsync(
                "UPDATE SalesCommissionBatch SET Status = 'PendingApproval' WHERE Id = @Id AND Status = 'Draft'",
                new { Id = batchId });
            return rows > 0;
        }

        // ── Approve / Reject ───────────────────────────────────────
        public async Task<bool> ApproveOrRejectAsync(
            int batchId, int approverLevel, int approvedById, string status, string? remarks)
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
                        await conn.ExecuteAsync(@"
                            INSERT INTO SalesCommissionApprovalHistory
                                (BatchId, ApproverLevel, ApprovedById, Status, Remarks)
                            VALUES (@BatchId, @Level, @ApprovedById, @Status, @Remarks)",
                            new { BatchId = batchId, Level = approverLevel, ApprovedById = approvedById, Status = status, Remarks = remarks }, tx);

                        string newStatus = status == "Rejected" ? "Rejected"
                            : approverLevel == 1 ? "L1Approved" : "Approved";

                        string requiredCurrentStatus = approverLevel == 1 ? "PendingApproval" : "L1Approved";
                        var rows = await conn.ExecuteAsync(@"
                            UPDATE SalesCommissionBatch SET Status = @NewStatus
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
                catch (SqlException ex) when (attempt < maxRetries &&
                    (ex.Number == 35 || ex.Number == -2 || ex.Number == 258 || ex.Class == 20))
                {
                    await Task.Delay(500 * (attempt + 1));
                }
            }
            return false;
        }

        // ── Pending Approvals ──────────────────────────────────────
        public async Task<IEnumerable<SalesCommissionBatch>> GetPendingApprovalsAsync(
            int approverLevel, int? approverUserId)
        {
            using var conn = new SqlConnection(_connStr);
            var requiredStatus = approverLevel == 1 ? "PendingApproval" : "L1Approved";
            return await conn.QueryAsync<SalesCommissionBatch>(@"
                SELECT b.*, u.FullName AS UserName, u.EmployeeCode,
                       g.FullName AS GeneratedByName
                FROM SalesCommissionBatch b
                INNER JOIN UserMaster u ON u.Id = b.UserId
                INNER JOIN UserMaster g ON g.Id = b.GeneratedById
                WHERE b.Status = @Status
                ORDER BY b.GeneratedAt DESC",
                new { Status = requiredStatus });
        }

        // ── Settlement ─────────────────────────────────────────────
        public async Task<bool> SettleBatchAsync(SalesCommSettlementFormViewModel model, int settledById)
        {
            using var conn = new SqlConnection(_connStr);
            var rows = await conn.ExecuteAsync(@"
                UPDATE SalesCommissionBatch
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

        /// Eligible payments: non-voided payments on invoices assigned to this sales user,
        /// within date range, not already in a non-Rejected batch line.
        private const string EligiblePaymentsSql = @"
            SELECT
                ip.Id AS InvoicePaymentId,
                i.Id AS InvoiceId,
                i.InvoiceNo,
                i.PartyName,
                ip.PaymentDate,
                ip.TotalAmountPaid AS PaymentAmount,
                a.ProductId,
                p.ProductName
            FROM InvoicePayment ip
            INNER JOIN Invoice i ON i.Id = ip.InvoiceId
            INNER JOIN SalesInvoiceAssignment a ON a.InvoiceId = i.Id AND a.SalesUserId = @SalesUserId
            LEFT JOIN ProductMaster p ON p.Id = a.ProductId
            WHERE ip.IsVoided = 0
              AND ip.PaymentDate BETWEEN @FromDate AND @ToDate
              AND NOT EXISTS (
                  SELECT 1 FROM SalesCommissionBatchLine bl
                  INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
                  WHERE bl.InvoicePaymentId = ip.Id AND b.Status <> 'Rejected'
              )
            ORDER BY ip.PaymentDate, i.InvoiceNo";

        private static decimal ResolveCommission(
            EligiblePayment pay, SalesCommissionConfiguration config, List<SalesCommissionRule> rules)
        {
            var (rate, _, commType) = ResolveRateAndSource(pay, config, rules);
            return commType == "Percentage"
                ? Math.Round(pay.PaymentAmount * rate / 100m, 2)
                : rate;
        }

        private static (decimal Rate, string Source, string CommType) ResolveRateAndSource(
            EligiblePayment pay, SalesCommissionConfiguration config, List<SalesCommissionRule> rules)
        {
            // Rules ordered by Priority DESC — product rule (10) beats default (0)
            foreach (var r in rules)
            {
                if (r.ProductId.HasValue && pay.ProductId.HasValue && r.ProductId == pay.ProductId)
                    return (r.Rate, "Product", r.CommissionType);
                if (!r.ProductId.HasValue)
                    return (r.Rate, "Default", r.CommissionType);
            }
            return (config.DefaultRate, "Default", config.CommissionType);
        }

        private class EligiblePayment
        {
            public int InvoicePaymentId { get; set; }
            public int InvoiceId { get; set; }
            public string InvoiceNo { get; set; } = string.Empty;
            public string PartyName { get; set; } = string.Empty;
            public DateTime PaymentDate { get; set; }
            public decimal PaymentAmount { get; set; }
            public int? ProductId { get; set; }
            public string? ProductName { get; set; }
        }
    }
}

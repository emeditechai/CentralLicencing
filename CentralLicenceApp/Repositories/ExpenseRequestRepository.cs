using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ExpenseRequestRepository : IExpenseRequestRepository
    {
        private readonly string _connectionString;

        public ExpenseRequestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ExpenseRequest>> GetRequestsForEmployeeAsync(int employeeId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(BaseRequestSql + " WHERE r.EmployeeId = @EmployeeId ORDER BY r.CreatedAt DESC", new { EmployeeId = employeeId });
        }

        public async Task<IEnumerable<ExpenseRequest>> GetRequestsForEmployeesAsync(IEnumerable<int> employeeIds)
        {
            var scopedEmployeeIds = employeeIds?
                .Where(employeeId => employeeId > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (scopedEmployeeIds.Length == 0)
            {
                return Enumerable.Empty<ExpenseRequest>();
            }

            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(
                BaseRequestSql + " WHERE r.EmployeeId IN @EmployeeIds ORDER BY r.CreatedAt DESC",
                new { EmployeeIds = scopedEmployeeIds });
        }

        public async Task<IEnumerable<ExpenseRequest>> GetPendingApprovalsAsync(int approverId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(BaseRequestSql + " WHERE r.ApproverId = @ApproverId AND r.Status = @Status ORDER BY r.SubmittedAt ASC, r.CreatedAt ASC", new { ApproverId = approverId, Status = ExpenseRequestStatus.PendingApproval });
        }

        public async Task<IEnumerable<ExpenseRequest>> GetPendingApprovalsAsync(IEnumerable<int> employeeIds)
        {
            var scopedEmployeeIds = employeeIds?
                .Where(employeeId => employeeId > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (scopedEmployeeIds.Length == 0)
            {
                return Enumerable.Empty<ExpenseRequest>();
            }

            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(
                BaseRequestSql + " WHERE r.EmployeeId IN @EmployeeIds AND r.Status = @Status ORDER BY r.SubmittedAt ASC, r.CreatedAt ASC",
                new
                {
                    EmployeeIds = scopedEmployeeIds,
                    Status = ExpenseRequestStatus.PendingApproval
                });
        }

        public async Task<IEnumerable<ExpenseRequest>> GetFinanceQueueAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(BaseRequestSql + @"
                WHERE r.Status IN @Statuses
                ORDER BY CASE
                    WHEN r.Status = @Approved THEN 1
                    WHEN r.Status = @ReimbursementInProcess THEN 2
                    WHEN r.Status = @Settled THEN 3
                    ELSE 4
                END,
                ISNULL(r.ReimbursementStartedAt, r.ApprovedAt) DESC,
                r.CreatedAt DESC",
                new
                {
                    Statuses = new[]
                    {
                        ExpenseRequestStatus.Approved,
                        ExpenseRequestStatus.ReimbursementInProcess,
                        ExpenseRequestStatus.Settled
                    },
                    Approved = ExpenseRequestStatus.Approved,
                    ReimbursementInProcess = ExpenseRequestStatus.ReimbursementInProcess,
                    Settled = ExpenseRequestStatus.Settled
                });
        }

        public async Task<IEnumerable<ExpenseRequest>> GetFinanceQueueAsync(IEnumerable<int> employeeIds)
        {
            var scopedEmployeeIds = employeeIds?
                .Where(employeeId => employeeId > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (scopedEmployeeIds.Length == 0)
            {
                return Enumerable.Empty<ExpenseRequest>();
            }

            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(BaseRequestSql + @"
                WHERE r.EmployeeId IN @EmployeeIds
                  AND r.Status IN @Statuses
                ORDER BY CASE
                    WHEN r.Status = @Approved THEN 1
                    WHEN r.Status = @ReimbursementInProcess THEN 2
                    WHEN r.Status = @Settled THEN 3
                    ELSE 4
                END,
                ISNULL(r.ReimbursementStartedAt, r.ApprovedAt) DESC,
                r.CreatedAt DESC",
                new
                {
                    EmployeeIds = scopedEmployeeIds,
                    Statuses = new[]
                    {
                        ExpenseRequestStatus.Approved,
                        ExpenseRequestStatus.ReimbursementInProcess,
                        ExpenseRequestStatus.Settled
                    },
                    Approved = ExpenseRequestStatus.Approved,
                    ReimbursementInProcess = ExpenseRequestStatus.ReimbursementInProcess,
                    Settled = ExpenseRequestStatus.Settled
                });
        }

        public async Task<(int Approved, int ReimbursementInProcess, int Settled)> GetDashboardCountsAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            var counts = await conn.QuerySingleAsync<ExpenseDashboardCounts>(@"
                SELECT
                    SUM(CASE WHEN Status = @Approved THEN 1 ELSE 0 END) AS Approved,
                    SUM(CASE WHEN Status = @ReimbursementInProcess THEN 1 ELSE 0 END) AS ReimbursementInProcess,
                    SUM(CASE WHEN Status = @Settled THEN 1 ELSE 0 END) AS Settled
                FROM ExpenseRequest",
                new
                {
                    Approved = ExpenseRequestStatus.Approved,
                    ReimbursementInProcess = ExpenseRequestStatus.ReimbursementInProcess,
                    Settled = ExpenseRequestStatus.Settled
                });

            return (counts.Approved, counts.ReimbursementInProcess, counts.Settled);
        }

        public async Task<(int Approved, int ReimbursementInProcess, int Settled)> GetDashboardCountsAsync(IEnumerable<int> employeeIds)
        {
            var scopedEmployeeIds = employeeIds?
                .Where(employeeId => employeeId > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (scopedEmployeeIds.Length == 0)
            {
                return (0, 0, 0);
            }

            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            var counts = await conn.QuerySingleAsync<ExpenseDashboardCounts>(@"
                SELECT
                    SUM(CASE WHEN Status = @Approved THEN 1 ELSE 0 END) AS Approved,
                    SUM(CASE WHEN Status = @ReimbursementInProcess THEN 1 ELSE 0 END) AS ReimbursementInProcess,
                    SUM(CASE WHEN Status = @Settled THEN 1 ELSE 0 END) AS Settled
                FROM ExpenseRequest
                WHERE EmployeeId IN @EmployeeIds",
                new
                {
                    EmployeeIds = scopedEmployeeIds,
                    Approved = ExpenseRequestStatus.Approved,
                    ReimbursementInProcess = ExpenseRequestStatus.ReimbursementInProcess,
                    Settled = ExpenseRequestStatus.Settled
                });

            return (counts.Approved, counts.ReimbursementInProcess, counts.Settled);
        }

        public async Task<IEnumerable<ExpenseRequest>> GetAllAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequest>(BaseRequestSql + " ORDER BY r.CreatedAt DESC");
        }

        public async Task<ExpenseRequest?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QuerySingleOrDefaultAsync<ExpenseRequest>(BaseRequestSql + " WHERE r.Id = @Id", new { Id = id });
        }

        public async Task<IEnumerable<ExpenseRequestLine>> GetLinesAsync(int requestId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var lines = (await conn.QueryAsync<ExpenseRequestLine>(@"
                SELECT l.*, c.CategoryName AS ExpenseCategoryName
                FROM ExpenseRequestLine l
                LEFT JOIN ExpenseCategoryMaster c ON l.ExpenseCategoryId = c.Id
                WHERE l.RequestId = @RequestId
                ORDER BY l.ExpenseDate DESC, l.Id DESC", new { RequestId = requestId })).ToList();

            await PopulateAttachmentsAsync(conn, lines);
            return lines;
        }

        public async Task<ExpenseRequestLine?> GetLineByIdAsync(int lineId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var line = await conn.QuerySingleOrDefaultAsync<ExpenseRequestLine>(@"
                SELECT l.*, c.CategoryName AS ExpenseCategoryName
                FROM ExpenseRequestLine l
                LEFT JOIN ExpenseCategoryMaster c ON l.ExpenseCategoryId = c.Id
                WHERE l.Id = @Id", new { Id = lineId });
            if (line != null)
            {
                line.Attachments = (await GetAttachmentsForLineAsync(line.Id)).ToList();
                EnsureLegacyAttachmentFallback(line);
            }

            return line;
        }

        public async Task<IEnumerable<ExpenseRequestLineAttachment>> GetAttachmentsForLineAsync(int lineId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequestLineAttachment>(@"
                SELECT Id, RequestLineId, FilePath, OriginalFileName, CreatedAt
                FROM ExpenseRequestLineAttachment
                WHERE RequestLineId = @RequestLineId
                ORDER BY CreatedAt ASC, Id ASC", new { RequestLineId = lineId });
        }

        public async Task<IEnumerable<ExpenseRequestApprovalHistory>> GetHistoryAsync(int requestId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ExpenseRequestApprovalHistory>(@"
                SELECT h.Id, h.RequestId, h.ActionTaken, h.ActionByUserId,
                       ISNULL(u.FullName, u.Username) AS ActionByName,
                       h.Remarks, h.ActionAt
                FROM ExpenseRequestApprovalHistory h
                LEFT JOIN UserMaster u ON h.ActionByUserId = u.Id
                WHERE h.RequestId = @RequestId
                ORDER BY h.ActionAt DESC, h.Id DESC", new { RequestId = requestId });
        }

        public async Task<int> CreateDraftAsync(ExpenseRequest request)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var sql = @"
                INSERT INTO ExpenseRequest
                    (RequestNumber, EmployeeId, ApproverId, PurposeOfTravel, EmployeeRemarks, Status, TotalAmount, ItemCount, CreatedAt, FinancialYearId, SettlementNotRequired)
                VALUES
                    ('', @EmployeeId, @ApproverId, @PurposeOfTravel, @EmployeeRemarks, @Status, 0, 0, @CreatedAt, @FinancialYearId, @SettlementNotRequired);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            request.CreatedAt = DateTime.Now;
            request.Status = ExpenseRequestStatus.Draft;
            using var tx = conn.BeginTransaction();
            var id = await conn.ExecuteScalarAsync<int>(sql, request, tx);
            var requestNumber = $"EXP-{request.CreatedAt:yyyyMMdd}-{id:0000}";
            await conn.ExecuteAsync("UPDATE ExpenseRequest SET RequestNumber = @RequestNumber WHERE Id = @Id", new { RequestNumber = requestNumber, Id = id }, tx);
            await AddHistoryAsync(conn, tx, id, request.EmployeeId, "Draft Created", "Request created as draft.");
            tx.Commit();
            return id;
        }

        public async Task<bool> UpdateDraftAsync(ExpenseRequest request)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET PurposeOfTravel = @PurposeOfTravel,
                    EmployeeRemarks = @EmployeeRemarks,
                    SettlementNotRequired = @SettlementNotRequired
                WHERE Id = @Id AND EmployeeId = @EmployeeId AND Status = @Status",
                new
                {
                    request.Id,
                    request.EmployeeId,
                    request.PurposeOfTravel,
                    request.EmployeeRemarks,
                    request.SettlementNotRequired,
                    Status = ExpenseRequestStatus.Draft
                });
            return rows > 0;
        }

        public async Task<bool> DeleteDraftAsync(int id, int employeeId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            var status = await conn.ExecuteScalarAsync<string?>("SELECT Status FROM ExpenseRequest WHERE Id = @Id AND EmployeeId = @EmployeeId", new { Id = id, EmployeeId = employeeId }, tx);
            if (!string.Equals(status, ExpenseRequestStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                tx.Rollback();
                return false;
            }

            await conn.ExecuteAsync("DELETE FROM ExpenseRequestApprovalHistory WHERE RequestId = @Id", new { Id = id }, tx);
            await conn.ExecuteAsync("DELETE FROM ExpenseRequestLine WHERE RequestId = @Id", new { Id = id }, tx);
            var rows = await conn.ExecuteAsync("DELETE FROM ExpenseRequest WHERE Id = @Id", new { Id = id }, tx);
            tx.Commit();
            return rows > 0;
        }

        public async Task<int> AddLineAsync(ExpenseRequestLine line)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            line.CreatedAt = DateTime.Now;
            var id = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ExpenseRequestLine
                    (RequestId, ItemType, ExpenseCategoryId, Title, ProjectOrCostCenter, ExpenseDate, CurrencyCode, Amount,
                     PayableAmountInr, AccommodationCountry, AccommodationCity, CheckInDate, CheckOutDate, ReceiptPath, Notes, CreatedAt)
                VALUES
                    (@RequestId, @ItemType, @ExpenseCategoryId, @Title, @ProjectOrCostCenter, @ExpenseDate, @CurrencyCode, @Amount,
                     @PayableAmountInr, @AccommodationCountry, @AccommodationCity, @CheckInDate, @CheckOutDate, @ReceiptPath, @Notes, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", line, tx);

            await RefreshTotalsAsync(conn, tx, line.RequestId);
            await AddHistoryAsync(conn, tx, line.RequestId, null, $"{line.ItemType} line added", line.Title);
            tx.Commit();
            return id;
        }

        public async Task AddLineAttachmentsAsync(int lineId, IEnumerable<ExpenseRequestLineAttachment> attachments)
        {
            var attachmentList = attachments?.Where(attachment => !string.IsNullOrWhiteSpace(attachment.FilePath)).ToList()
                ?? new List<ExpenseRequestLineAttachment>();
            if (!attachmentList.Any())
            {
                return;
            }

            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();

            foreach (var attachment in attachmentList)
            {
                attachment.RequestLineId = lineId;
                if (attachment.CreatedAt == default)
                {
                    attachment.CreatedAt = DateTime.Now;
                }
            }

            await conn.ExecuteAsync(@"
                INSERT INTO ExpenseRequestLineAttachment (RequestLineId, FilePath, OriginalFileName, CreatedAt)
                VALUES (@RequestLineId, @FilePath, @OriginalFileName, @CreatedAt);", attachmentList, tx);

            var primaryPath = attachmentList.First().FilePath;
            await conn.ExecuteAsync(@"
                UPDATE ExpenseRequestLine
                SET ReceiptPath = ISNULL(NULLIF(ReceiptPath, ''), @ReceiptPath)
                WHERE Id = @Id", new { Id = lineId, ReceiptPath = primaryPath }, tx);

            tx.Commit();
        }

        public async Task<bool> UpdateLineAsync(ExpenseRequestLine line)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequestLine
                SET ItemType = @ItemType,
                    ExpenseCategoryId = @ExpenseCategoryId,
                    Title = @Title,
                    ProjectOrCostCenter = @ProjectOrCostCenter,
                    ExpenseDate = @ExpenseDate,
                    CurrencyCode = @CurrencyCode,
                    Amount = @Amount,
                    PayableAmountInr = @PayableAmountInr,
                    AccommodationCountry = @AccommodationCountry,
                    AccommodationCity = @AccommodationCity,
                    CheckInDate = @CheckInDate,
                    CheckOutDate = @CheckOutDate,
                    ReceiptPath = @ReceiptPath,
                    Notes = @Notes
                WHERE Id = @Id", line, tx);

            if (rows > 0)
            {
                await RefreshTotalsAsync(conn, tx, line.RequestId);
                await AddHistoryAsync(conn, tx, line.RequestId, null, $"{line.ItemType} line updated", line.Title);
            }

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> DeleteLineAsync(int lineId, int requestId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            var rows = await conn.ExecuteAsync("DELETE FROM ExpenseRequestLine WHERE Id = @Id AND RequestId = @RequestId", new { Id = lineId, RequestId = requestId }, tx);
            if (rows > 0)
            {
                await RefreshTotalsAsync(conn, tx, requestId);
                await AddHistoryAsync(conn, tx, requestId, null, "Line deleted", null);
            }
            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> SubmitAsync(int requestId, int employeeId, int? approverId, bool autoApprove, string? remarks)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();

            var request = await conn.QuerySingleOrDefaultAsync<ExpenseRequest>("SELECT * FROM ExpenseRequest WHERE Id = @Id AND EmployeeId = @EmployeeId", new { Id = requestId, EmployeeId = employeeId }, tx);
            if (request == null || !string.Equals(request.Status, ExpenseRequestStatus.Draft, StringComparison.OrdinalIgnoreCase))
            {
                tx.Rollback();
                return false;
            }

            var lineCount = await conn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM ExpenseRequestLine WHERE RequestId = @RequestId", new { RequestId = requestId }, tx);
            if (lineCount == 0)
            {
                tx.Rollback();
                return false;
            }

            if (autoApprove)
            {
                await conn.ExecuteAsync(@"
                    UPDATE ExpenseRequest
                    SET Status = @Approved,
                        SubmittedAt = @Now,
                        ApprovedAt = @Now,
                        ApprovedById = @EmployeeId,
                        ApproverId = @EmployeeId,
                        ApprovalRemarks = @Remarks
                    WHERE Id = @Id", new { Approved = ExpenseRequestStatus.Approved, Now = DateTime.Now, EmployeeId = employeeId, Remarks = remarks, Id = requestId }, tx);
                await AddHistoryAsync(conn, tx, requestId, employeeId, "Submitted", remarks);
                await AddHistoryAsync(conn, tx, requestId, employeeId, "Auto Approved", "Core member request auto approved.");
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE ExpenseRequest
                    SET Status = @Pending,
                        SubmittedAt = @Now,
                        ApproverId = @ApproverId,
                        ApprovalRemarks = NULL,
                        ApprovedAt = NULL,
                        RejectedAt = NULL,
                        ApprovedById = NULL
                    WHERE Id = @Id", new { Pending = ExpenseRequestStatus.PendingApproval, Now = DateTime.Now, ApproverId = approverId, Id = requestId }, tx);
                await AddHistoryAsync(conn, tx, requestId, employeeId, "Submitted", remarks);
            }

            tx.Commit();
            return true;
        }

        public async Task<bool> ApproveAsync(int requestId, int actionByUserId, string? remarks)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET Status = @Approved,
                    ApprovedAt = @Now,
                    ApprovedById = @ActionByUserId,
                    ApprovalRemarks = @Remarks
                WHERE Id = @Id AND Status = @Pending", new { Approved = ExpenseRequestStatus.Approved, Now = DateTime.Now, ActionByUserId = actionByUserId, Remarks = remarks, Id = requestId, Pending = ExpenseRequestStatus.PendingApproval }, tx);

            if (rows > 0)
                await AddHistoryAsync(conn, tx, requestId, actionByUserId, "Approved", remarks);

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> RejectAsync(int requestId, int actionByUserId, string? remarks)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();
            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET Status = @Rejected,
                    RejectedAt = @Now,
                    ApprovedById = @ActionByUserId,
                    ApprovalRemarks = @Remarks
                WHERE Id = @Id AND Status = @Pending", new { Rejected = ExpenseRequestStatus.Rejected, Now = DateTime.Now, ActionByUserId = actionByUserId, Remarks = remarks, Id = requestId, Pending = ExpenseRequestStatus.PendingApproval }, tx);

            if (rows > 0)
                await AddHistoryAsync(conn, tx, requestId, actionByUserId, "Rejected", remarks);

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> StartReimbursementAsync(int requestId, int actionByUserId, string remarks)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();

            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET Status = @Status,
                    ReimbursementStartedAt = @Now,
                    ReimbursementStartedById = @ActionByUserId,
                    ReimbursementRemarks = @Remarks
                WHERE Id = @Id AND Status = @Approved",
                new
                {
                    Status = ExpenseRequestStatus.ReimbursementInProcess,
                    Now = DateTime.Now,
                    ActionByUserId = actionByUserId,
                    Remarks = remarks,
                    Id = requestId,
                    Approved = ExpenseRequestStatus.Approved
                }, tx);

            if (rows > 0)
            {
                await AddHistoryAsync(conn, tx, requestId, actionByUserId, "Reimbursement Started", remarks);
            }

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> SettleAsync(int requestId, int actionByUserId, DateTime settlementDate, decimal settlementAmount, string settlementMode, string settlementReferenceNo, string? settlementRemarks)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();

            var request = await conn.QuerySingleOrDefaultAsync<ExpenseRequest>(
                "SELECT * FROM ExpenseRequest WHERE Id = @Id",
                new { Id = requestId },
                tx);

            if (request == null || !string.Equals(request.Status, ExpenseRequestStatus.ReimbursementInProcess, StringComparison.OrdinalIgnoreCase))
            {
                tx.Rollback();
                return false;
            }

            var receiptNumber = string.IsNullOrWhiteSpace(request.SettlementReceiptNumber)
                ? $"SET-{DateTime.Now:yyyyMMdd}-{requestId:0000}"
                : request.SettlementReceiptNumber;

            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET Status = @Settled,
                    SettlementDate = @SettlementDate,
                    SettledAt = @Now,
                    SettledById = @ActionByUserId,
                    SettlementAmount = @SettlementAmount,
                    SettlementMode = @SettlementMode,
                    SettlementReferenceNo = @SettlementReferenceNo,
                    SettlementRemarks = @SettlementRemarks,
                    SettlementReceiptNumber = @SettlementReceiptNumber
                WHERE Id = @Id AND Status = @ReimbursementInProcess",
                new
                {
                    Settled = ExpenseRequestStatus.Settled,
                    SettlementDate = settlementDate,
                    Now = DateTime.Now,
                    ActionByUserId = actionByUserId,
                    SettlementAmount = settlementAmount,
                    SettlementMode = settlementMode,
                    SettlementReferenceNo = settlementReferenceNo,
                    SettlementRemarks = settlementRemarks,
                    SettlementReceiptNumber = receiptNumber,
                    Id = requestId,
                    ReimbursementInProcess = ExpenseRequestStatus.ReimbursementInProcess
                }, tx);

            if (rows > 0)
            {
                await AddHistoryAsync(conn, tx, requestId, actionByUserId, "Settled", settlementRemarks ?? $"Settled amount {settlementAmount:N2} via {settlementMode}.");
            }

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> AutoSettleAsync(int requestId, int actionByUserId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            using var tx = conn.BeginTransaction();

            var request = await conn.QuerySingleOrDefaultAsync<ExpenseRequest>(
                "SELECT * FROM ExpenseRequest WHERE Id = @Id",
                new { Id = requestId },
                tx);

            if (request == null || !string.Equals(request.Status, ExpenseRequestStatus.Approved, StringComparison.OrdinalIgnoreCase))
            {
                tx.Rollback();
                return false;
            }

            var receiptNumber = $"SET-{DateTime.Now:yyyyMMdd}-{requestId:0000}";
            var now = DateTime.Now;

            var rows = await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET Status = @Settled,
                    ReimbursementStartedAt = @Now,
                    ReimbursementStartedById = @ActionByUserId,
                    ReimbursementRemarks = 'Settlement not required – Company expense (Auto-settled)',
                    SettlementDate = CAST(@Now AS DATE),
                    SettledAt = @Now,
                    SettledById = @ActionByUserId,
                    SettlementAmount = TotalAmount,
                    SettlementMode = 'N/A',
                    SettlementReferenceNo = 'AUTO',
                    SettlementRemarks = 'Auto-settled: Settlement not required (Company expense)',
                    SettlementReceiptNumber = @SettlementReceiptNumber
                WHERE Id = @Id AND Status = @Approved",
                new
                {
                    Settled = ExpenseRequestStatus.Settled,
                    Now = now,
                    ActionByUserId = actionByUserId,
                    SettlementReceiptNumber = receiptNumber,
                    Id = requestId,
                    Approved = ExpenseRequestStatus.Approved
                }, tx);

            if (rows > 0)
            {
                await AddHistoryAsync(conn, tx, requestId, actionByUserId, "Auto Settled", "Settlement not required – Company expense. Auto-settled on approval.");
            }

            tx.Commit();
            return rows > 0;
        }

        private async Task RefreshTotalsAsync(IDbConnection conn, IDbTransaction tx, int requestId)
        {
            var summary = await conn.QuerySingleAsync<RequestSummary>(@"
                SELECT ISNULL(SUM(Amount), 0) AS TotalAmount, COUNT(1) AS ItemCount
                FROM ExpenseRequestLine
                WHERE RequestId = @RequestId", new { RequestId = requestId }, tx);

            await conn.ExecuteAsync(@"
                UPDATE ExpenseRequest
                SET TotalAmount = @TotalAmount,
                    ItemCount = @ItemCount
                WHERE Id = @RequestId", new { summary.TotalAmount, summary.ItemCount, RequestId = requestId }, tx);
        }

        private static Task AddHistoryAsync(IDbConnection conn, IDbTransaction tx, int requestId, int? actionByUserId, string actionTaken, string? remarks)
        {
            return conn.ExecuteAsync(@"
                INSERT INTO ExpenseRequestApprovalHistory (RequestId, ActionTaken, ActionByUserId, Remarks, ActionAt)
                VALUES (@RequestId, @ActionTaken, @ActionByUserId, @Remarks, @ActionAt)",
                new { RequestId = requestId, ActionTaken = actionTaken, ActionByUserId = actionByUserId, Remarks = remarks, ActionAt = DateTime.Now }, tx);
        }

        private const string BaseRequestSql = @"
            SELECT r.*, 
                   ISNULL(e.FullName, e.Username) AS EmployeeName,
                   e.EmployeeCode,
                 ISNULL(a.FullName, a.Username) AS ApproverName,
                 ISNULL(rs.FullName, rs.Username) AS ReimbursementStartedByName,
                 ISNULL(st.FullName, st.Username) AS SettledByName,
                 fy.FYCode
            FROM ExpenseRequest r
            INNER JOIN UserMaster e ON r.EmployeeId = e.Id
             LEFT JOIN UserMaster a ON r.ApproverId = a.Id
             LEFT JOIN UserMaster rs ON r.ReimbursementStartedById = rs.Id
             LEFT JOIN UserMaster st ON r.SettledById = st.Id
             LEFT JOIN FinancialYearMaster fy ON fy.Id = r.FinancialYearId";

        private sealed class RequestSummary
        {
            public decimal TotalAmount { get; set; }
            public int ItemCount { get; set; }
        }

        private sealed class ExpenseDashboardCounts
        {
            public int Approved { get; set; }
            public int ReimbursementInProcess { get; set; }
            public int Settled { get; set; }
        }

        private static async Task PopulateAttachmentsAsync(IDbConnection conn, List<ExpenseRequestLine> lines)
        {
            if (!lines.Any())
            {
                return;
            }

            var lineIds = lines.Select(line => line.Id).ToArray();
            var attachments = (await conn.QueryAsync<ExpenseRequestLineAttachment>(@"
                SELECT Id, RequestLineId, FilePath, OriginalFileName, CreatedAt
                FROM ExpenseRequestLineAttachment
                WHERE RequestLineId IN @LineIds
                ORDER BY CreatedAt ASC, Id ASC", new { LineIds = lineIds })).ToList();

            var attachmentsByLineId = attachments
                .GroupBy(attachment => attachment.RequestLineId)
                .ToDictionary(group => group.Key, group => group.ToList());

            foreach (var line in lines)
            {
                line.Attachments = attachmentsByLineId.TryGetValue(line.Id, out var lineAttachments)
                    ? lineAttachments
                    : new List<ExpenseRequestLineAttachment>();

                EnsureLegacyAttachmentFallback(line);
            }
        }

        private static void EnsureLegacyAttachmentFallback(ExpenseRequestLine line)
        {
            if (line.Attachments.Any() || string.IsNullOrWhiteSpace(line.ReceiptPath))
            {
                return;
            }

            line.Attachments.Add(new ExpenseRequestLineAttachment
            {
                RequestLineId = line.Id,
                FilePath = line.ReceiptPath,
                OriginalFileName = System.IO.Path.GetFileName(line.ReceiptPath),
                CreatedAt = line.CreatedAt
            });
        }
    }
}
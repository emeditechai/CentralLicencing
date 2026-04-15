using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public class SalesInvoiceAssignmentRepository : ISalesInvoiceAssignmentRepository
    {
        private readonly string _connStr;
        public SalesInvoiceAssignmentRepository(string connStr) => _connStr = connStr;

        public async Task<IEnumerable<SalesInvoiceAssignmentRow>> GetAssignmentsAsync(
            int? salesUserId, DateTime? from, DateTime? to)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT
                    a.Id AS AssignmentId, i.Id AS InvoiceId, i.InvoiceNo, i.InvoiceDate,
                    i.PartyName, i.TotalAmount, i.ReceivedAmount, i.Status AS InvoiceStatus,
                    a.SalesUserId, u.FullName AS SalesUserName,
                    a.ProductId, p.ProductName,
                    a.AssignedAt, ab.FullName AS AssignedByName,
                    ISNULL(lc.TotalCommission, 0) AS TotalCommissionAmount
                FROM SalesInvoiceAssignment a
                INNER JOIN Invoice i ON i.Id = a.InvoiceId
                INNER JOIN UserMaster u ON u.Id = a.SalesUserId
                LEFT JOIN ProductMaster p ON p.Id = a.ProductId
                INNER JOIN UserMaster ab ON ab.Id = a.AssignedById
                OUTER APPLY (
                    SELECT SUM(al.CommissionAmount) AS TotalCommission
                    FROM SalesInvoiceAssignmentLine al WHERE al.AssignmentId = a.Id
                ) lc
                WHERE 1 = 1";

            var p2 = new DynamicParameters();
            if (salesUserId.HasValue) { sql += " AND a.SalesUserId = @SalesUserId"; p2.Add("SalesUserId", salesUserId.Value); }
            if (from.HasValue) { sql += " AND i.InvoiceDate >= @From"; p2.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND i.InvoiceDate <= @To"; p2.Add("To", to.Value); }

            sql += " ORDER BY i.InvoiceDate DESC, a.AssignedAt DESC";
            return await conn.QueryAsync<SalesInvoiceAssignmentRow>(sql, p2);
        }

        public async Task<IEnumerable<SalesInvoiceAssignmentRow>> GetUnassignedInvoicesAsync(
            DateTime? from, DateTime? to)
        {
            using var conn = new SqlConnection(_connStr);
            // Show invoices available for assignment.
            // Exclude invoices whose commission is already settled (batch Status = 'Paid').
            var sql = @"
                SELECT
                    NULL AS AssignmentId, i.Id AS InvoiceId, i.InvoiceNo, i.InvoiceDate,
                    i.PartyName, i.TotalAmount, i.ReceivedAmount, i.Status AS InvoiceStatus,
                    NULL AS SalesUserId, NULL AS SalesUserName,
                    NULL AS ProductId, NULL AS ProductName,
                    NULL AS AssignedAt, NULL AS AssignedByName,
                    ISNULL(ac.UserCount, 0) AS AssignedUserCount,
                    0 AS TotalCommissionAmount
                FROM Invoice i
                OUTER APPLY (
                    SELECT COUNT(DISTINCT a.SalesUserId) AS UserCount
                    FROM SalesInvoiceAssignment a WHERE a.InvoiceId = i.Id
                ) ac
                WHERE i.Status IN ('Sent','Paid','Partial')
                  AND NOT EXISTS (
                      SELECT 1
                      FROM SalesCommissionBatchLine bl
                      INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
                      WHERE bl.InvoiceId = i.Id AND b.Status = 'Paid'
                  )";

            var p2 = new DynamicParameters();
            if (from.HasValue) { sql += " AND i.InvoiceDate >= @From"; p2.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND i.InvoiceDate <= @To"; p2.Add("To", to.Value); }

            sql += " ORDER BY i.InvoiceDate DESC";
            return await conn.QueryAsync<SalesInvoiceAssignmentRow>(sql, p2);
        }

        public async Task<IEnumerable<InvoiceLineItemDto>> GetInvoiceLineItemsAsync(int invoiceId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<InvoiceLineItemDto>(@"
                SELECT
                    il.Id AS InvoiceLineId,
                    il.SNo,
                    il.ItemDescription,
                    il.PlanName,
                    il.Qty,
                    il.Rate,
                    il.Amount,
                    (ISNULL(il.CgstAmount, 0) + ISNULL(il.SgstAmount, 0) + ISNULL(il.IgstAmount, 0)) AS GstAmount,
                    il.Amount AS NetAmount
                FROM InvoiceLine il
                WHERE il.InvoiceId = @InvoiceId
                ORDER BY il.SNo",
                new { InvoiceId = invoiceId });
        }

        public async Task<bool> AssignWithLinesAsync(AssignInvoiceRequest request, int assignedById)
        {
            if (request.Lines == null || !request.Lines.Any())
                return false;

            using var conn = new SqlConnection(_connStr);
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();

            try
            {
                // Insert the assignment header
                var assignmentId = await conn.QuerySingleAsync<int>(@"
                    INSERT INTO SalesInvoiceAssignment (InvoiceId, SalesUserId, ProductId, AssignedById, AssignedAt)
                    VALUES (@InvoiceId, @SalesUserId, NULL, @AssignedById, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new { request.InvoiceId, request.SalesUserId, AssignedById = assignedById }, tx);

                // Insert line items
                foreach (var line in request.Lines)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO SalesInvoiceAssignmentLine
                            (AssignmentId, InvoiceLineId, ItemDescription, NetAmount,
                             CommissionType, CommissionRate, CommissionAmount)
                        VALUES
                            (@AssignmentId, @InvoiceLineId, @ItemDescription, @NetAmount,
                             @CommissionType, @CommissionRate, @CommissionAmount)",
                        new
                        {
                            AssignmentId = assignmentId,
                            line.InvoiceLineId,
                            line.ItemDescription,
                            line.NetAmount,
                            line.CommissionType,
                            line.CommissionRate,
                            line.CommissionAmount
                        }, tx);
                }

                tx.Commit();
                return true;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                tx.Rollback();
                return false;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> UnassignAsync(int assignmentId)
        {
            using var conn = new SqlConnection(_connStr);
            // Block if any payment from this invoice is in an active batch line
            var assignment = await conn.QueryFirstOrDefaultAsync<SalesInvoiceAssignment>(
                "SELECT * FROM SalesInvoiceAssignment WHERE Id = @Id",
                new { Id = assignmentId });
            if (assignment == null) return false;

            var inBatch = await conn.QueryFirstOrDefaultAsync<int?>(
                @"SELECT TOP 1 bl.Id
                  FROM SalesCommissionBatchLine bl
                  INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
                  WHERE bl.InvoiceId = @InvoiceId AND b.Status <> 'Rejected'",
                new { assignment.InvoiceId });

            if (inBatch.HasValue) return false;

            // CASCADE will delete SalesInvoiceAssignmentLine rows
            return await conn.ExecuteAsync(
                "DELETE FROM SalesInvoiceAssignment WHERE Id = @Id",
                new { Id = assignmentId }) > 0;
        }

        public async Task<IEnumerable<SalesInvoiceAssignmentLine>> GetAssignmentLinesAsync(int assignmentId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesInvoiceAssignmentLine>(@"
                SELECT * FROM SalesInvoiceAssignmentLine
                WHERE AssignmentId = @AssignmentId
                ORDER BY Id",
                new { AssignmentId = assignmentId });
        }

        public async Task<IEnumerable<SalesInvoiceAssignmentRow>> GetAssignmentsForInvoiceAsync(int invoiceId)
        {
            using var conn = new SqlConnection(_connStr);
            return await conn.QueryAsync<SalesInvoiceAssignmentRow>(@"
                SELECT
                    a.Id AS AssignmentId, i.Id AS InvoiceId, i.InvoiceNo, i.InvoiceDate,
                    i.PartyName, i.TotalAmount, i.ReceivedAmount, i.Status AS InvoiceStatus,
                    a.SalesUserId, u.FullName AS SalesUserName,
                    a.ProductId, p.ProductName,
                    a.AssignedAt, ab.FullName AS AssignedByName,
                    ISNULL(lc.TotalCommission, 0) AS TotalCommissionAmount
                FROM SalesInvoiceAssignment a
                INNER JOIN Invoice i ON i.Id = a.InvoiceId
                INNER JOIN UserMaster u ON u.Id = a.SalesUserId
                LEFT JOIN ProductMaster p ON p.Id = a.ProductId
                INNER JOIN UserMaster ab ON ab.Id = a.AssignedById
                OUTER APPLY (
                    SELECT SUM(al.CommissionAmount) AS TotalCommission
                    FROM SalesInvoiceAssignmentLine al WHERE al.AssignmentId = a.Id
                ) lc
                WHERE a.InvoiceId = @InvoiceId
                ORDER BY a.AssignedAt DESC",
                new { InvoiceId = invoiceId });
        }
    }
}

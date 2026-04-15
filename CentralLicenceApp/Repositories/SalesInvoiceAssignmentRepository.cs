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
                    a.AssignedAt, ab.FullName AS AssignedByName
                FROM SalesInvoiceAssignment a
                INNER JOIN Invoice i ON i.Id = a.InvoiceId
                INNER JOIN UserMaster u ON u.Id = a.SalesUserId
                LEFT JOIN ProductMaster p ON p.Id = a.ProductId
                INNER JOIN UserMaster ab ON ab.Id = a.AssignedById
                WHERE 1 = 1";

            var p2 = new DynamicParameters();
            if (salesUserId.HasValue) { sql += " AND a.SalesUserId = @SalesUserId"; p2.Add("SalesUserId", salesUserId.Value); }
            if (from.HasValue) { sql += " AND i.InvoiceDate >= @From"; p2.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND i.InvoiceDate <= @To"; p2.Add("To", to.Value); }

            sql += " ORDER BY i.InvoiceDate DESC";
            return await conn.QueryAsync<SalesInvoiceAssignmentRow>(sql, p2);
        }

        public async Task<IEnumerable<SalesInvoiceAssignmentRow>> GetUnassignedInvoicesAsync(
            DateTime? from, DateTime? to)
        {
            using var conn = new SqlConnection(_connStr);
            var sql = @"
                SELECT
                    NULL AS AssignmentId, i.Id AS InvoiceId, i.InvoiceNo, i.InvoiceDate,
                    i.PartyName, i.TotalAmount, i.ReceivedAmount, i.Status AS InvoiceStatus,
                    NULL AS SalesUserId, NULL AS SalesUserName,
                    NULL AS ProductId, NULL AS ProductName,
                    NULL AS AssignedAt, NULL AS AssignedByName
                FROM Invoice i
                WHERE i.Status IN ('Sent','Paid','Partial')
                  AND NOT EXISTS (SELECT 1 FROM SalesInvoiceAssignment a WHERE a.InvoiceId = i.Id)";

            var p2 = new DynamicParameters();
            if (from.HasValue) { sql += " AND i.InvoiceDate >= @From"; p2.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND i.InvoiceDate <= @To"; p2.Add("To", to.Value); }

            sql += " ORDER BY i.InvoiceDate DESC";
            return await conn.QueryAsync<SalesInvoiceAssignmentRow>(sql, p2);
        }

        public async Task<bool> AssignAsync(int invoiceId, int salesUserId, int? productId, int assignedById)
        {
            using var conn = new SqlConnection(_connStr);
            try
            {
                return await conn.ExecuteAsync(@"
                    INSERT INTO SalesInvoiceAssignment (InvoiceId, SalesUserId, ProductId, AssignedById, AssignedAt)
                    VALUES (@InvoiceId, @SalesUserId, @ProductId, @AssignedById, GETDATE())",
                    new { InvoiceId = invoiceId, SalesUserId = salesUserId, ProductId = productId, AssignedById = assignedById }) > 0;
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                // Unique constraint violation — already assigned
                return false;
            }
        }

        public async Task<bool> UnassignAsync(int invoiceId)
        {
            using var conn = new SqlConnection(_connStr);
            // Only allow unassign if payment is not in an active commission batch line
            var inBatch = await conn.QueryFirstOrDefaultAsync<int?>(
                @"SELECT TOP 1 bl.Id
                  FROM SalesCommissionBatchLine bl
                  INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
                  INNER JOIN SalesInvoiceAssignment a ON a.InvoiceId = bl.InvoiceId
                  WHERE a.InvoiceId = @InvoiceId AND b.Status <> 'Rejected'",
                new { InvoiceId = invoiceId });

            if (inBatch.HasValue) return false;

            return await conn.ExecuteAsync(
                "DELETE FROM SalesInvoiceAssignment WHERE InvoiceId = @InvoiceId",
                new { InvoiceId = invoiceId }) > 0;
        }

        public async Task<bool> ReassignAsync(int invoiceId, int newSalesUserId, int? productId, int assignedById)
        {
            using var conn = new SqlConnection(_connStr);
            // Check if invoice is in an active commission batch
            var inBatch = await conn.QueryFirstOrDefaultAsync<int?>(
                @"SELECT TOP 1 bl.Id
                  FROM SalesCommissionBatchLine bl
                  INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
                  INNER JOIN SalesInvoiceAssignment a ON a.InvoiceId = bl.InvoiceId
                  WHERE a.InvoiceId = @InvoiceId AND b.Status <> 'Rejected'",
                new { InvoiceId = invoiceId });

            if (inBatch.HasValue) return false;

            return await conn.ExecuteAsync(@"
                UPDATE SalesInvoiceAssignment
                SET SalesUserId = @SalesUserId, ProductId = @ProductId,
                    AssignedById = @AssignedById, AssignedAt = GETDATE()
                WHERE InvoiceId = @InvoiceId",
                new { InvoiceId = invoiceId, SalesUserId = newSalesUserId, ProductId = productId, AssignedById = assignedById }) > 0;
        }
    }
}

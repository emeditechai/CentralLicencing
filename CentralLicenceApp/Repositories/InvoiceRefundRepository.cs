using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class InvoiceRefundRepository : IInvoiceRefundRepository
    {
        private readonly string _connectionString;

        public InvoiceRefundRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<InvoiceRefund>> GetByPaymentIdAsync(int paymentId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<InvoiceRefund>(@"
                SELECT r.*, cn.Id AS CreditNoteId, cn.CreditNoteNo
                FROM   InvoiceRefund r
                LEFT JOIN CreditNote cn ON cn.RefundId = r.Id
                WHERE  r.PaymentId = @PaymentId
                ORDER  BY r.CreatedAt",
                new { PaymentId = paymentId });
        }

        public async Task<InvoiceRefund?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<InvoiceRefund>(@"
                SELECT r.*, cn.Id AS CreditNoteId, cn.CreditNoteNo
                FROM   InvoiceRefund r
                LEFT JOIN CreditNote cn ON cn.RefundId = r.Id
                WHERE  r.Id = @Id",
                new { Id = id });
        }

        public async Task<string> GetNextRefundNoAsync()
        {
            using var conn = CreateConnection();
            var year     = DateTime.Now.Year % 100;
            var nextYear = year + 1;
            var prefix   = $"REF/{year:D2}-{nextYear:D2}/";

            var last = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 RefundNo FROM InvoiceRefund WHERE RefundNo LIKE @Prefix ORDER BY Id DESC",
                new { Prefix = prefix + "%" });

            int seq = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('/');
                if (parts.Length >= 3 && int.TryParse(parts[^1], out var n))
                    seq = n + 1;
            }

            return $"{prefix}{seq:D4}";
        }

        public async Task<int> CreateAsync(InvoiceRefund refund)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            refund.CreatedAt = DateTime.Now;

            var refundId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO InvoiceRefund
                    (RefundNo, PaymentId, InvoiceId, InvoiceNo, PartyId, PartyName,
                     RefundDate, Amount, PaymentModeId, PaymentModeName, ReferenceNo,
                     Remarks, CreatedBy, CreatedAt, FinancialYearId)
                VALUES
                    (@RefundNo, @PaymentId, @InvoiceId, @InvoiceNo, @PartyId, @PartyName,
                     @RefundDate, @Amount, @PaymentModeId, @PaymentModeName, @ReferenceNo,
                     @Remarks, @CreatedBy, @CreatedAt, @FinancialYearId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                refund, tx);

            // NOTE: The invoice ReceivedAmount was already reversed when the payment was voided.
            // A refund only records the physical cash return to the customer — no invoice balance change.

            tx.Commit();
            return refundId;
        }
    }
}

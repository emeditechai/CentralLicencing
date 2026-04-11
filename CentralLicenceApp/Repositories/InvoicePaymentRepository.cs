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
    public class InvoicePaymentRepository : IInvoicePaymentRepository
    {
        private readonly string _connectionString;

        public InvoicePaymentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<InvoicePayment>> GetAllAsync()
        {
            using var conn = CreateConnection();
            var payments = (await conn.QueryAsync<InvoicePayment>(@"
                SELECT p.*, i.Status AS InvoiceStatus, fy.FYCode
                FROM   InvoicePayment p
                INNER  JOIN Invoice i ON i.Id = p.InvoiceId
                LEFT   JOIN FinancialYearMaster fy ON fy.Id = p.FinancialYearId
                ORDER  BY p.CreatedAt DESC")).ToList();

            if (payments.Any())
            {
                var ids = payments.Select(p => p.Id).ToArray();
                var lines = await conn.QueryAsync<InvoicePaymentLine>(
                    "SELECT * FROM InvoicePaymentLine WHERE PaymentId IN @Ids",
                    new { Ids = ids });
                var refunds = await conn.QueryAsync<InvoiceRefund>(
                    "SELECT * FROM InvoiceRefund WHERE PaymentId IN @Ids",
                    new { Ids = ids });

                foreach (var pmt in payments)
                {
                    pmt.Lines   = lines.Where(l => l.PaymentId == pmt.Id).ToList();
                    pmt.Refunds = refunds.Where(r => r.PaymentId == pmt.Id).ToList();
                }
            }

            return payments;
        }

        public async Task<IEnumerable<InvoicePayment>> GetByInvoiceIdAsync(int invoiceId)
        {
            using var conn = CreateConnection();
            var payments = (await conn.QueryAsync<InvoicePayment>(@"
                SELECT p.*, i.Status AS InvoiceStatus
                FROM   InvoicePayment p
                INNER  JOIN Invoice i ON i.Id = p.InvoiceId
                WHERE  p.InvoiceId = @InvoiceId
                ORDER  BY p.CreatedAt DESC",
                new { InvoiceId = invoiceId })).ToList();

            if (payments.Any())
            {
                var ids = payments.Select(p => p.Id).ToArray();
                var lines = await conn.QueryAsync<InvoicePaymentLine>(
                    "SELECT * FROM InvoicePaymentLine WHERE PaymentId IN @Ids",
                    new { Ids = ids });
                var refunds = await conn.QueryAsync<InvoiceRefund>(
                    "SELECT * FROM InvoiceRefund WHERE PaymentId IN @Ids",
                    new { Ids = ids });

                foreach (var pmt in payments)
                {
                    pmt.Lines   = lines.Where(l => l.PaymentId == pmt.Id).ToList();
                    pmt.Refunds = refunds.Where(r => r.PaymentId == pmt.Id).ToList();
                }
            }

            return payments;
        }

        public async Task<InvoicePayment?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            var payment = await conn.QuerySingleOrDefaultAsync<InvoicePayment>(@"
                SELECT p.*, i.Status AS InvoiceStatus
                FROM   InvoicePayment p
                INNER  JOIN Invoice i ON i.Id = p.InvoiceId
                WHERE  p.Id = @Id",
                new { Id = id });

            if (payment != null)
            {
                payment.Lines = (await conn.QueryAsync<InvoicePaymentLine>(
                    "SELECT * FROM InvoicePaymentLine WHERE PaymentId = @Id",
                    new { Id = id })).ToList();
                payment.Refunds = (await conn.QueryAsync<InvoiceRefund>(
                    "SELECT * FROM InvoiceRefund WHERE PaymentId = @Id ORDER BY CreatedAt",
                    new { Id = id })).ToList();
            }

            return payment;
        }

        public async Task<string> GetNextReceiptNoAsync()
        {
            using var conn = CreateConnection();
            var year = DateTime.Now.Year % 100;
            var nextYear = year + 1;
            var prefix = $"RCT/{year:D2}-{nextYear:D2}/";

            var last = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 ReceiptNo FROM InvoicePayment WHERE ReceiptNo LIKE @Prefix ORDER BY Id DESC",
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

        public async Task<int> CreateAsync(InvoicePayment payment)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            payment.CreatedAt = DateTime.Now;

            var paymentId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO InvoicePayment
                    (ReceiptNo, InvoiceId, InvoiceNo, PartyId, PartyName,
                     PaymentDate, TotalAmountPaid, Notes, CreatedBy, CreatedAt, FinancialYearId)
                VALUES
                    (@ReceiptNo, @InvoiceId, @InvoiceNo, @PartyId, @PartyName,
                     @PaymentDate, @TotalAmountPaid, @Notes, @CreatedBy, @CreatedAt, @FinancialYearId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                payment, tx);

            foreach (var line in payment.Lines)
            {
                line.PaymentId = paymentId;
                await conn.ExecuteAsync(@"
                    INSERT INTO InvoicePaymentLine
                        (PaymentId, PaymentModeId, PaymentModeName, Amount, ReferenceNo,
                         CardType, CardLastFour, BankId, BankName)
                    VALUES
                        (@PaymentId, @PaymentModeId, @PaymentModeName, @Amount, @ReferenceNo,
                         @CardType, @CardLastFour, @BankId, @BankName);",
                    line, tx);
            }

            // Update Invoice.ReceivedAmount and Status
            await conn.ExecuteAsync(@"
                UPDATE Invoice
                SET    ReceivedAmount = ReceivedAmount + @Paid,
                       Status = CASE
                           WHEN (TotalAmount - (ReceivedAmount + @Paid)) <= 0
                               THEN 'Paid'
                           ELSE 'Partial'
                       END
                WHERE  Id = @InvoiceId
                  AND  Status NOT IN ('Cancelled')",
                new { Paid = payment.TotalAmountPaid, InvoiceId = payment.InvoiceId }, tx);

            // Cascade-recalculate PreviousBalance for ALL open invoices of this party.
            // Each invoice's PreviousBalance = sum of (TotalAmount - ReceivedAmount)
            // for all earlier invoices (by Id) that are not yet Paid or Cancelled.
            // This keeps PreviousBalance accurate after every payment, eliminating stale snapshots.
            await conn.ExecuteAsync(@"
                UPDATE inv
                SET    inv.PreviousBalance = ISNULL((
                           SELECT SUM(i2.TotalAmount - i2.ReceivedAmount)
                           FROM   Invoice i2
                           WHERE  i2.PartyId = inv.PartyId
                             AND  i2.Id      < inv.Id
                             AND  i2.Status NOT IN ('Paid', 'Cancelled')
                       ), 0)
                FROM   Invoice inv
                WHERE  inv.PartyId = @PartyId
                  AND  inv.Status  NOT IN ('Cancelled')",
                new { PartyId = payment.PartyId }, tx);

            tx.Commit();
            return paymentId;
        }

        public async Task<bool> HasActivePaymentsAsync(int invoiceId)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM InvoicePayment WHERE InvoiceId = @InvoiceId AND IsVoided = 0",
                new { InvoiceId = invoiceId });
            return count > 0;
        }

        public async Task<bool> VoidAsync(int id, string voidedBy, string remarks)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            // Load the payment
            var payment = await conn.QuerySingleOrDefaultAsync<InvoicePayment>(
                "SELECT * FROM InvoicePayment WHERE Id = @Id AND IsVoided = 0",
                new { Id = id }, tx);

            if (payment == null) return false;

            // Mark payment as voided
            await conn.ExecuteAsync(@"
                UPDATE InvoicePayment
                SET    IsVoided    = 1,
                       VoidedAt   = @VoidedAt,
                       VoidedBy   = @VoidedBy,
                       VoidRemarks = @Remarks
                WHERE  Id = @Id",
                new { Id = id, VoidedAt = DateTime.Now, VoidedBy = voidedBy, Remarks = remarks }, tx);

            // Reverse the payment on the invoice
            await conn.ExecuteAsync(@"
                UPDATE Invoice
                SET    ReceivedAmount = CASE WHEN ReceivedAmount - @Paid < 0 THEN 0
                                             ELSE ReceivedAmount - @Paid END,
                       Status = CASE
                           WHEN (TotalAmount - CASE WHEN ReceivedAmount - @Paid < 0 THEN 0 ELSE ReceivedAmount - @Paid END) <= 0
                               THEN 'Paid'
                           WHEN CASE WHEN ReceivedAmount - @Paid < 0 THEN 0 ELSE ReceivedAmount - @Paid END > 0
                               THEN 'Partial'
                           ELSE 'Sent'
                       END
                WHERE  Id     = @InvoiceId
                  AND  Status NOT IN ('Cancelled')",
                new { Paid = payment.TotalAmountPaid, InvoiceId = payment.InvoiceId }, tx);

            tx.Commit();
            return true;
        }
    }
}

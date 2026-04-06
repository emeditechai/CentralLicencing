using System;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class CreditNoteRepository : ICreditNoteRepository
    {
        private readonly string _connectionString;

        public CreditNoteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<string> GetNextCreditNoteNoAsync()
        {
            using var conn = CreateConnection();
            var year     = DateTime.Now.Year % 100;
            var nextYear = year + 1;
            var prefix   = $"CN/{year:D2}-{nextYear:D2}/";

            var last = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 CreditNoteNo FROM CreditNote WHERE CreditNoteNo LIKE @Prefix ORDER BY Id DESC",
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

        public async Task<int> CreateAsync(CreditNote creditNote)
        {
            using var conn = CreateConnection();
            creditNote.CreatedAt = DateTime.Now;

            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO CreditNote
                    (CreditNoteNo, RefundId, RefundNo, PaymentId, ReceiptNo,
                     InvoiceId, InvoiceNo, PartyId, PartyName,
                     PartyAddress, PartyGSTINNo, PartyPANNo, PartyContactPerson, PartyMobile,
                     CreditNoteDate, Amount, PaymentModeId, PaymentModeName,
                     ReferenceNo, Reason, CreatedBy, CreatedAt)
                VALUES
                    (@CreditNoteNo, @RefundId, @RefundNo, @PaymentId, @ReceiptNo,
                     @InvoiceId, @InvoiceNo, @PartyId, @PartyName,
                     @PartyAddress, @PartyGSTINNo, @PartyPANNo, @PartyContactPerson, @PartyMobile,
                     @CreditNoteDate, @Amount, @PaymentModeId, @PaymentModeName,
                     @ReferenceNo, @Reason, @CreatedBy, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                creditNote);
        }

        public async Task<CreditNote?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<CreditNote>(
                "SELECT * FROM CreditNote WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<CreditNote?> GetByRefundIdAsync(int refundId)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<CreditNote>(
                "SELECT * FROM CreditNote WHERE RefundId = @RefundId",
                new { RefundId = refundId });
        }
    }
}

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
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            using var conn = CreateConnection();
            var invoices = (await conn.QueryAsync<Invoice>(@"
                SELECT * FROM Invoice ORDER BY CreatedAt DESC")).ToList();

            if (invoices.Any())
            {
                var ids = invoices.Select(i => i.Id).ToArray();
                var lines = await conn.QueryAsync<InvoiceLine>(
                    "SELECT * FROM InvoiceLine WHERE InvoiceId IN @Ids ORDER BY SNo",
                    new { Ids = ids });

                foreach (var inv in invoices)
                    inv.Lines = lines.Where(l => l.InvoiceId == inv.Id).ToList();
            }

            return invoices;
        }

        public async Task<Invoice?> GetByInvoiceNoAsync(string invoiceNo)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<Invoice>(
                "SELECT * FROM Invoice WHERE InvoiceNo = @InvoiceNo",
                new { InvoiceNo = invoiceNo.Trim() });
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            var inv = await conn.QuerySingleOrDefaultAsync<Invoice>(
                "SELECT * FROM Invoice WHERE Id = @Id", new { Id = id });

            if (inv != null)
            {
                inv.Lines = (await conn.QueryAsync<InvoiceLine>(
                    "SELECT * FROM InvoiceLine WHERE InvoiceId = @Id ORDER BY SNo",
                    new { Id = id })).ToList();

                // Load signatory IDs
                inv.SignatoryUserIds = (await conn.QueryAsync<int>(
                    "SELECT UserId FROM InvoiceSignatories WHERE InvoiceId = @Id ORDER BY SortOrder",
                    new { Id = id })).ToList();

                // Load full signatory user objects
                if (inv.SignatoryUserIds.Any())
                    inv.Signatories = (await conn.QueryAsync<UserMaster>(
                        @"SELECT u.Id, u.Username, u.FullName, u.DigitalSignaturePath
                          FROM UserMaster u
                          INNER JOIN InvoiceSignatories s ON s.UserId = u.Id
                          WHERE s.InvoiceId = @Id
                          ORDER BY s.SortOrder",
                        new { Id = id })).ToList();

                // Populate TermsAndConditions from master template
                if (inv.TermsConditionTemplateId.HasValue)
                {
                    inv.TermsAndConditions = await conn.ExecuteScalarAsync<string?>(
                        "SELECT Description FROM TermsConditionTemplate WHERE Id = @Id",
                        new { Id = inv.TermsConditionTemplateId.Value });
                }
            }

            return inv;
        }

        public async Task<string> GetNextInvoiceNoAsync()
        {
            using var conn = CreateConnection();
            var year = DateTime.Now.Year % 100;
            var nextYear = year + 1;
            var prefix = $"EL/SL/{year:D2}-{nextYear:D2}/";

            var last = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 InvoiceNo FROM Invoice WHERE InvoiceNo LIKE @Prefix ORDER BY Id DESC",
                new { Prefix = prefix + "%" });

            int seq = 1;
            if (!string.IsNullOrEmpty(last))
            {
                var parts = last.Split('/');
                if (parts.Length >= 4 && int.TryParse(parts[^1], out var n))
                    seq = n + 1;
            }

            return $"{prefix}{seq:D4}";
        }

        public async Task<decimal> GetPartyOutstandingBalanceAsync(int partyId, int? excludeInvoiceId = null)
        {
            using var conn = CreateConnection();
            // Sum of (TotalAmount - ReceivedAmount + PreviousBalance) for all active invoices of the party
            // excluding Paid and Cancelled invoices, and optionally the invoice being edited
            return await conn.ExecuteScalarAsync<decimal>(@"
                SELECT ISNULL(SUM(TotalAmount - ReceivedAmount), 0)
                FROM   Invoice
                WHERE  PartyId = @PartyId
                  AND  Status NOT IN ('Paid', 'Cancelled')
                  AND  (@ExcludeId IS NULL OR Id <> @ExcludeId)",
                new { PartyId = partyId, ExcludeId = excludeInvoiceId });
        }

        public async Task<int> CreateAsync(Invoice invoice)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            invoice.CreatedAt = DateTime.Now;

            var invoiceId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Invoice
                    (InvoiceNo, InvoiceDate, DueDate, QuotationId, QuotationNo, PartyId, PartyName, PartyAddress, PartyGSTINNo, PartyPANNo,
                     PartyContactPerson, PartyMobile, Notes, TermsConditionTemplateId,
                     SubTotal, TotalCgst, TotalSgst, TotalIgst, EnableRoundOff, RoundOff, TotalAmount,
                     ReceivedAmount, PreviousBalance, Status, CreatedBy, CreatedAt)
                VALUES
                    (@InvoiceNo, @InvoiceDate, @DueDate, @QuotationId, @QuotationNo, @PartyId, @PartyName, @PartyAddress, @PartyGSTINNo, @PartyPANNo,
                     @PartyContactPerson, @PartyMobile, @Notes, @TermsConditionTemplateId,
                     @SubTotal, @TotalCgst, @TotalSgst, @TotalIgst, @EnableRoundOff, @RoundOff, @TotalAmount,
                     @ReceivedAmount, @PreviousBalance, @Status, @CreatedBy, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                invoice, tx);

            foreach (var line in invoice.Lines)
            {
                line.InvoiceId = invoiceId;
                await conn.ExecuteAsync(@"
                    INSERT INTO InvoiceLine
                        (InvoiceId, SNo, ItemDescription, PlanName, Type, Qty, Rate,
                         DiscountPercent, DiscountAmount, Amount, GstPercent, CgstAmount, SgstAmount, IgstAmount)
                    VALUES
                        (@InvoiceId, @SNo, @ItemDescription, @PlanName, @Type, @Qty, @Rate,
                         @DiscountPercent, @DiscountAmount, @Amount, @GstPercent, @CgstAmount, @SgstAmount, @IgstAmount);",
                    line, tx);
            }

            // Sync signatories
            await conn.ExecuteAsync(
                "DELETE FROM InvoiceSignatories WHERE InvoiceId = @InvoiceId",
                new { InvoiceId = invoiceId }, tx);
            var sigIds = invoice.SignatoryUserIds.Distinct().Take(3).ToList();
            for (int idx = 0; idx < sigIds.Count; idx++)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO InvoiceSignatories (InvoiceId, UserId, SortOrder) VALUES (@InvoiceId, @UserId, @SortOrder)",
                    new { InvoiceId = invoiceId, UserId = sigIds[idx], SortOrder = idx }, tx);
            }

            tx.Commit();
            return invoiceId;
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            var rows = await conn.ExecuteAsync(@"
                UPDATE Invoice SET
                    InvoiceDate        = @InvoiceDate,
                    DueDate            = @DueDate,
                    PartyId            = @PartyId,
                    PartyName          = @PartyName,
                    PartyAddress       = @PartyAddress,
                    PartyGSTINNo       = @PartyGSTINNo,
                    PartyPANNo         = @PartyPANNo,
                    PartyContactPerson = @PartyContactPerson,
                    PartyMobile        = @PartyMobile,
                    Notes              = @Notes,
                    TermsConditionTemplateId = @TermsConditionTemplateId,
                    SubTotal           = @SubTotal,
                    TotalCgst          = @TotalCgst,
                    TotalSgst          = @TotalSgst,
                    TotalIgst          = @TotalIgst,
                    EnableRoundOff     = @EnableRoundOff,
                    RoundOff           = @RoundOff,
                    TotalAmount        = @TotalAmount,
                    ReceivedAmount     = @ReceivedAmount,
                    PreviousBalance    = @PreviousBalance,
                    Status             = @Status
                WHERE Id = @Id",
                invoice, tx);

            await conn.ExecuteAsync(
                "DELETE FROM InvoiceLine WHERE InvoiceId = @Id",
                new { invoice.Id }, tx);

            foreach (var line in invoice.Lines)
            {
                line.InvoiceId = invoice.Id;
                await conn.ExecuteAsync(@"
                    INSERT INTO InvoiceLine
                        (InvoiceId, SNo, ItemDescription, PlanName, Type, Qty, Rate,
                         DiscountPercent, DiscountAmount, Amount, GstPercent, CgstAmount, SgstAmount, IgstAmount)
                    VALUES
                        (@InvoiceId, @SNo, @ItemDescription, @PlanName, @Type, @Qty, @Rate,
                         @DiscountPercent, @DiscountAmount, @Amount, @GstPercent, @CgstAmount, @SgstAmount, @IgstAmount);",
                    line, tx);
            }

            // Sync signatories
            await conn.ExecuteAsync(
                "DELETE FROM InvoiceSignatories WHERE InvoiceId = @Id",
                new { invoice.Id }, tx);
            var sigIds2 = invoice.SignatoryUserIds.Distinct().Take(3).ToList();
            for (int idx = 0; idx < sigIds2.Count; idx++)
            {
                await conn.ExecuteAsync(
                    "INSERT INTO InvoiceSignatories (InvoiceId, UserId, SortOrder) VALUES (@InvoiceId, @UserId, @SortOrder)",
                    new { InvoiceId = invoice.Id, UserId = sigIds2[idx], SortOrder = idx }, tx);
            }

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE Invoice SET Status = @Status WHERE Id = @Id",
                new { Id = id, Status = status }) > 0;
        }

        public async Task<bool> CancelAsync(int id, string remarks)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE Invoice SET Status = 'Cancelled', CancelRemarks = @Remarks WHERE Id = @Id AND Status != 'Cancelled'",
                new { Id = id, Remarks = remarks }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM InvoiceSignatories WHERE InvoiceId = @Id", new { Id = id }, tx);
            await conn.ExecuteAsync("DELETE FROM InvoiceLine WHERE InvoiceId = @Id", new { Id = id }, tx);
            var rows = await conn.ExecuteAsync("DELETE FROM Invoice WHERE Id = @Id", new { Id = id }, tx);
            tx.Commit();
            return rows > 0;
        }
    }
}

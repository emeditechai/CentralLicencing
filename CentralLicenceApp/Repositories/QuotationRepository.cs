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
    public class QuotationRepository : IQuotationRepository
    {
        private readonly string _connectionString;

        public QuotationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Quotation>> GetAllAsync()
        {
            using var conn = CreateConnection();
            var quotations = (await conn.QueryAsync<Quotation>(@"
                SELECT * FROM Quotation ORDER BY CreatedAt DESC")).ToList();

            if (quotations.Any())
            {
                var ids = quotations.Select(q => q.Id).ToArray();
                var lines = await conn.QueryAsync<QuotationLine>(
                    "SELECT * FROM QuotationLine WHERE QuotationId IN @Ids ORDER BY SNo",
                    new { Ids = ids });

                foreach (var q in quotations)
                    q.Lines = lines.Where(l => l.QuotationId == q.Id).ToList();
            }

            return quotations;
        }

        public async Task<Quotation?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            var q = await conn.QuerySingleOrDefaultAsync<Quotation>(
                "SELECT * FROM Quotation WHERE Id = @Id", new { Id = id });

            if (q != null)
            {
                q.Lines = (await conn.QueryAsync<QuotationLine>(
                    "SELECT * FROM QuotationLine WHERE QuotationId = @Id ORDER BY SNo",
                    new { Id = id })).ToList();

                q.SignatoryUserIds = (await conn.QueryAsync<int>(
                    "SELECT UserId FROM QuotationSignatories WHERE QuotationId = @Id ORDER BY SortOrder",
                    new { Id = id })).ToList();

                if (q.SignatoryUserIds.Any())
                {
                    q.Signatories = (await conn.QueryAsync<UserMaster>(
                        @"SELECT u.Id, u.FullName, u.Username, u.DigitalSignaturePath
                          FROM UserMaster u
                          INNER JOIN QuotationSignatories qs ON qs.UserId = u.Id
                          WHERE qs.QuotationId = @Id
                          ORDER BY qs.SortOrder",
                        new { Id = id })).ToList();
                }

                // Populate TermsAndConditions from master template
                if (q.TermsConditionTemplateId.HasValue)
                {
                    q.TermsAndConditions = await conn.ExecuteScalarAsync<string?>(
                        "SELECT Description FROM TermsConditionTemplate WHERE Id = @Id",
                        new { Id = q.TermsConditionTemplateId.Value });
                }
            }

            return q;
        }

        public async Task<string> GetNextQuotationNoAsync()
        {
            using var conn = CreateConnection();
            var year = DateTime.Now.Year % 100;
            var nextYear = year + 1;
            var prefix = $"QT/{year:D2}-{nextYear:D2}/";

            var last = await conn.ExecuteScalarAsync<string?>(
                "SELECT TOP 1 QuotationNo FROM Quotation WHERE QuotationNo LIKE @Prefix ORDER BY Id DESC",
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

        public async Task<int> CreateAsync(Quotation quotation)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            quotation.CreatedAt = DateTime.Now;

            var quotationId = await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO Quotation
                    (QuotationNo, QuotationDate, ValidUntilDate, PartyId, PartyName, PartyAddress, PartyGSTINNo, PartyPANNo,
                     PartyContactPerson, PartyMobile, Notes, TermsConditionTemplateId,
                     SubTotal, TotalCgst, TotalSgst, TotalIgst, EnableRoundOff, RoundOff, TotalAmount, Status, CreatedBy, CreatedAt)
                VALUES
                    (@QuotationNo, @QuotationDate, @ValidUntilDate, @PartyId, @PartyName, @PartyAddress, @PartyGSTINNo, @PartyPANNo,
                     @PartyContactPerson, @PartyMobile, @Notes, @TermsConditionTemplateId,
                     @SubTotal, @TotalCgst, @TotalSgst, @TotalIgst, @EnableRoundOff, @RoundOff, @TotalAmount, @Status, @CreatedBy, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                quotation, tx);

            foreach (var line in quotation.Lines)
            {
                line.QuotationId = quotationId;
                await conn.ExecuteAsync(@"
                    INSERT INTO QuotationLine
                        (QuotationId, SNo, ItemDescription, PlanName, Type, Qty, Rate,
                         DiscountPercent, DiscountAmount, Amount, GstPercent, CgstAmount, SgstAmount, IgstAmount)
                    VALUES
                        (@QuotationId, @SNo, @ItemDescription, @PlanName, @Type, @Qty, @Rate,
                         @DiscountPercent, @DiscountAmount, @Amount, @GstPercent, @CgstAmount, @SgstAmount, @IgstAmount);",
                    line, tx);
            }

            // Sync signatories (max 3)
            await conn.ExecuteAsync(
                "DELETE FROM QuotationSignatories WHERE QuotationId = @Id",
                new { Id = quotationId }, tx);
            var sigIds = quotation.SignatoryUserIds.Distinct().Take(3).ToList();
            for (int i = 0; i < sigIds.Count; i++)
                await conn.ExecuteAsync(
                    "INSERT INTO QuotationSignatories (QuotationId, UserId, SortOrder) VALUES (@QId, @UId, @Ord)",
                    new { QId = quotationId, UId = sigIds[i], Ord = i }, tx);

            tx.Commit();
            return quotationId;
        }

        public async Task<bool> UpdateAsync(Quotation quotation)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();

            var rows = await conn.ExecuteAsync(@"
                UPDATE Quotation SET
                    QuotationDate      = @QuotationDate,
                    ValidUntilDate     = @ValidUntilDate,
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
                    Status             = @Status
                WHERE Id = @Id AND Status != 'Converted'",
                quotation, tx);

            await conn.ExecuteAsync(
                "DELETE FROM QuotationLine WHERE QuotationId = @Id",
                new { quotation.Id }, tx);

            foreach (var line in quotation.Lines)
            {
                line.QuotationId = quotation.Id;
                await conn.ExecuteAsync(@"
                    INSERT INTO QuotationLine
                        (QuotationId, SNo, ItemDescription, PlanName, Type, Qty, Rate,
                         DiscountPercent, DiscountAmount, Amount, GstPercent, CgstAmount, SgstAmount, IgstAmount)
                    VALUES
                        (@QuotationId, @SNo, @ItemDescription, @PlanName, @Type, @Qty, @Rate,
                         @DiscountPercent, @DiscountAmount, @Amount, @GstPercent, @CgstAmount, @SgstAmount, @IgstAmount);",
                    line, tx);
            }

            // Sync signatories (max 3)
            await conn.ExecuteAsync(
                "DELETE FROM QuotationSignatories WHERE QuotationId = @Id",
                new { quotation.Id }, tx);
            var sigIds2 = quotation.SignatoryUserIds.Distinct().Take(3).ToList();
            for (int i = 0; i < sigIds2.Count; i++)
                await conn.ExecuteAsync(
                    "INSERT INTO QuotationSignatories (QuotationId, UserId, SortOrder) VALUES (@QId, @UId, @Ord)",
                    new { QId = quotation.Id, UId = sigIds2[i], Ord = i }, tx);

            tx.Commit();
            return rows > 0;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE Quotation SET Status = @Status WHERE Id = @Id",
                new { Id = id, Status = status }) > 0;
        }

        public async Task<bool> CancelAsync(int id, string remarks)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "UPDATE Quotation SET Status = 'Cancelled', CancelRemarks = @Remarks WHERE Id = @Id AND Status != 'Cancelled'",
                new { Id = id, Remarks = remarks }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tx = conn.BeginTransaction();
            await conn.ExecuteAsync("DELETE FROM QuotationSignatories WHERE QuotationId = @Id", new { Id = id }, tx);
            await conn.ExecuteAsync("DELETE FROM QuotationLine WHERE QuotationId = @Id", new { Id = id }, tx);
            var rows = await conn.ExecuteAsync(
                "DELETE FROM Quotation WHERE Id = @Id AND Status != 'Converted'", new { Id = id }, tx);
            tx.Commit();
            return rows > 0;
        }
    }
}

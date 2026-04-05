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
    public class ClientDetailsRepository : IClientDetailsRepository
    {
        private readonly string _connectionString;

        public ClientDetailsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<ClientDetails?> GetByClientCodeAsync(string clientCode)
        {
            using var conn = CreateConnection();
            var details = await conn.QuerySingleOrDefaultAsync<ClientDetails>(
                @"SELECT ID, ClientCode, ClientPersonName, address AS Address,
                                                 ProductPurchased, DOB, Anniversarydate, IsInternalUse, ReferenceClientCode, IsActive
                  FROM ClientDetails WHERE ClientCode = @ClientCode",
                new { ClientCode = clientCode });

            if (details != null)
            {
                details.PurchasedProducts = (await GetPurchasedProductsByClientCodeAsync(clientCode)).ToList();
            }

            return details;
        }

        public async Task UpsertAsync(ClientDetails details)
        {
            using var conn = CreateConnection();
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            var existing = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ClientDetails WHERE ClientCode = @ClientCode",
                new { details.ClientCode }, tx);

            int clientDetailsId;

            if (existing == 0)
            {
                clientDetailsId = await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO ClientDetails
                        (ClientCode, ClientPersonName, address, ProductPurchased,
                        DOB, Anniversarydate, IsInternalUse, ReferenceClientCode, IsActive)
                    VALUES
                        (@ClientCode, @ClientPersonName, @Address, @ProductPurchased,
                        @DOB, @Anniversarydate, @IsInternalUse, @ReferenceClientCode, @IsActive);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    details, tx);
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE ClientDetails SET
                        ClientPersonName  = @ClientPersonName,
                        address           = @Address,
                        ProductPurchased  = @ProductPurchased,
                        DOB               = @DOB,
                        Anniversarydate   = @Anniversarydate,
                        IsInternalUse     = @IsInternalUse,
                        ReferenceClientCode = @ReferenceClientCode,
                        IsActive          = @IsActive
                    WHERE ClientCode = @ClientCode",
                    details, tx);

                clientDetailsId = await conn.ExecuteScalarAsync<int>(
                    "SELECT ID FROM ClientDetails WHERE ClientCode = @ClientCode",
                    new { details.ClientCode }, tx);
            }

            await conn.ExecuteAsync(
                "DELETE FROM ClientPurchasedProduct WHERE ClientDetailsId = @ClientDetailsId",
                new { ClientDetailsId = clientDetailsId }, tx);

            if (details.PurchasedProducts.Any())
            {
                const string insertPurchasedProductSql = @"
                    INSERT INTO ClientPurchasedProduct
                        (ClientDetailsId, ClientCode, ProductId, ProductRateId, ProductCode, ProductName, PricingModel, BillingModel, BillingFrequency, BasePrice,
                         AmcCalculationType, AmcPercentage, AmcAmount, InvoiceNo, IsActive, SubscriptionStartDate, CreatedAt)
                    VALUES
                        (@ClientDetailsId, @ClientCode, @ProductId, @ProductRateId, @ProductCode, @ProductName, @PricingModel, @BillingModel, @BillingFrequency, @BasePrice,
                         @AmcCalculationType, @AmcPercentage, @AmcAmount, @InvoiceNo, @IsActive, @SubscriptionStartDate, GETDATE());";

                foreach (var item in details.PurchasedProducts)
                {
                    item.ClientDetailsId = clientDetailsId;
                    item.ClientCode = details.ClientCode;
                    await conn.ExecuteAsync(insertPurchasedProductSql, item, tx);
                }
            }

            tx.Commit();
        }

        public async Task<IEnumerable<string>> GetClientCodesWithDetailsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<string>(
                "SELECT ClientCode FROM ClientDetails WHERE ClientCode IS NOT NULL");
        }

        public async Task<IReadOnlyList<ClientPurchasedProduct>> GetPurchasedProductsByClientCodeAsync(string clientCode)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<ClientPurchasedProduct>(@"
                SELECT
                    Id,
                    ClientDetailsId,
                    ClientCode,
                    ProductId,
                    ProductRateId,
                    ProductCode,
                    ProductName,
                    PricingModel,
                    BillingModel,
                    BillingFrequency,
                    BasePrice,
                    AmcCalculationType,
                    AmcPercentage,
                    AmcAmount,
                    InvoiceNo,
                    IsActive,
                    SubscriptionStartDate,
                    LastRenewedAt,
                    CreatedAt
                FROM ClientPurchasedProduct
                WHERE ClientCode = @ClientCode
                ORDER BY ProductName, PricingModel, BillingModel, BillingFrequency", new { ClientCode = clientCode });

            return items.ToList();
        }

        public async Task<IReadOnlyList<SubscriptionInvoiceReminder>> GetSubscriptionRemindersAsync(int daysAhead = 5)
        {
            using var conn = CreateConnection();

            // Fetch all active subscription products for active clients
            var rows = await conn.QueryAsync<SubscriptionProductRow>(@"
                SELECT
                    cpp.Id,
                    cpp.ClientCode,
                    ISNULL(
                        (SELECT TOP 1 ClientName
                         FROM   ClientAppLicense
                         WHERE  ClientCode = cpp.ClientCode AND IsActive = 1
                         ORDER BY CreatedAt DESC),
                        cpp.ClientCode) AS ClientName,
                    cpp.ProductName,
                    cpp.BillingFrequency,
                    cpp.BasePrice,
                    cpp.InvoiceNo,
                    cpp.LastRenewedAt,
                    COALESCE(cpp.SubscriptionStartDate, cpp.CreatedAt) AS StartDate
                FROM  ClientPurchasedProduct cpp
                INNER JOIN ClientDetails cd ON cd.ClientCode = cpp.ClientCode AND cd.IsActive = 1
                WHERE cpp.IsActive = 1
                  AND UPPER(cpp.BillingModel) = 'SUBSCRIPTION'
                  AND cpp.BillingFrequency IS NOT NULL
                  AND cpp.BillingFrequency <> ''");

            var today     = DateTime.Today;
            var reminders = new List<SubscriptionInvoiceReminder>();

            foreach (var row in rows)
            {
                var nextRenewal = GetNextRenewalDate(row.StartDate, row.BillingFrequency, today);
                if (nextRenewal == null) continue;

                var daysLeft = (nextRenewal.Value - today).Days;
                if (daysLeft < 0 || daysLeft > daysAhead) continue;

                // Suppress if already renewed within the current billing cycle.
                // Current cycle starts at (nextRenewal - 1 cycle). If LastRenewedAt
                // falls on or after that date, the user already acted on this renewal —
                // don't show it again until the next cycle window opens.
                if (row.LastRenewedAt.HasValue)
                {
                    var monthsPerCycle = row.BillingFrequency.Trim().ToUpperInvariant() switch
                    {
                        "MONTHLY"     => 1,
                        "QUARTERLY"   => 3,
                        "HALF YEARLY" => 6,
                        "ANNUAL"      => 12,
                        _             => 1
                    };
                    var cycleStart = AddMonthsSafe(nextRenewal.Value, -monthsPerCycle);
                    if (row.LastRenewedAt.Value.Date >= cycleStart.Date)
                        continue;
                }

                reminders.Add(new SubscriptionInvoiceReminder
                {
                    PurchasedProductId = row.Id,
                    ClientCode       = row.ClientCode,
                    ClientName       = row.ClientName,
                    ProductName      = row.ProductName,
                    BillingFrequency = row.BillingFrequency,
                    BasePrice        = row.BasePrice,
                    InvoiceNo        = row.InvoiceNo,
                    StartDate        = row.StartDate,
                    NextRenewalDate  = nextRenewal.Value,
                    DaysUntilRenewal = daysLeft
                });
            }

            return reminders.OrderBy(r => r.DaysUntilRenewal).ToList();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static DateTime? GetNextRenewalDate(DateTime startDate, string frequency, DateTime today)
        {
            var monthsPerCycle = frequency.Trim().ToUpperInvariant() switch
            {
                "MONTHLY"     => 1,
                "QUARTERLY"   => 3,
                "HALF YEARLY" => 6,
                "ANNUAL"      => 12,
                _             => 0
            };
            if (monthsPerCycle == 0) return null;

            // The first renewal is always one full cycle AFTER the start date.
            // Starting from startDate itself would cause a subscription created TODAY
            // to return today as "due", which is wrong — the first invoice isn't due until
            // one cycle from now.
            var candidate = AddMonthsSafe(startDate.Date, monthsPerCycle);

            // Advance forward until the candidate is on or after today
            while (candidate < today)
                candidate = AddMonthsSafe(candidate, monthsPerCycle);

            return candidate;
        }

        private static DateTime AddMonthsSafe(DateTime date, int months)
        {
            // AddMonths already handles day-overflow (e.g. Jan 31 + 1 month = Feb 28/29)
            return date.AddMonths(months);
        }

        // Private DTO for the Dapper query
        private class SubscriptionProductRow
        {
            public int Id { get; set; }
            public string ClientCode { get; set; } = string.Empty;
            public string ClientName { get; set; } = string.Empty;
            public string ProductName { get; set; } = string.Empty;
            public string BillingFrequency { get; set; } = string.Empty;
            public decimal BasePrice { get; set; }
            public string? InvoiceNo { get; set; }
            public DateTime? LastRenewedAt { get; set; }
            public DateTime StartDate { get; set; }
        }

        public async Task MarkRenewedAsync(int purchasedProductId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE ClientPurchasedProduct SET LastRenewedAt = GETDATE() WHERE Id = @Id",
                new { Id = purchasedProductId });
        }
    }
}

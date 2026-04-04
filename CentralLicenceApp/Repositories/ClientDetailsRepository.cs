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
                         AmcCalculationType, AmcPercentage, AmcAmount, InvoiceNo, IsActive, CreatedAt)
                    VALUES
                        (@ClientDetailsId, @ClientCode, @ProductId, @ProductRateId, @ProductCode, @ProductName, @PricingModel, @BillingModel, @BillingFrequency, @BasePrice,
                         @AmcCalculationType, @AmcPercentage, @AmcAmount, @InvoiceNo, @IsActive, GETDATE());";

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
                    CreatedAt
                FROM ClientPurchasedProduct
                WHERE ClientCode = @ClientCode
                ORDER BY ProductName, PricingModel, BillingModel, BillingFrequency", new { ClientCode = clientCode });

            return items.ToList();
        }
    }
}

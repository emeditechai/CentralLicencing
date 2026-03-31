using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ProductRateRepository : IProductRateRepository
    {
        private readonly string _connectionString;

        public ProductRateRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ProductRate>> GetAllAsync(int? productId = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProductRate>(@"
                SELECT
                    pr.Id,
                    pr.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductType,
                    pr.PricingModel,
                    pr.BillingModel,
                    pr.BillingFrequency,
                    pr.ProductSpecification,
                    pr.Features,
                    pr.Rate,
                    pr.AmcCalculationType,
                    pr.AmcPercentage,
                    pr.AmcAmount,
                    pr.IsActive,
                    pr.CreatedAt,
                    ISNULL(dis.DiscountOfferCount, 0) AS DiscountOfferCount,
                    ISNULL(dis.ActiveDiscountOfferCount, 0) AS ActiveDiscountOfferCount
                FROM ProductRate pr
                INNER JOIN ProductMaster p ON p.Id = pr.ProductId
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS DiscountOfferCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveDiscountOfferCount
                    FROM ProductRateDiscountOffer d
                    WHERE d.ProductRateId = pr.Id
                ) dis
                WHERE (@ProductId IS NULL OR pr.ProductId = @ProductId)
                ORDER BY p.ProductName, pr.PricingModel, pr.BillingModel, pr.BillingFrequency, pr.Rate", new { ProductId = productId });
        }

        public async Task<ProductRate?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ProductRate>(@"
                SELECT
                    pr.Id,
                    pr.ProductId,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductType,
                    pr.PricingModel,
                    pr.BillingModel,
                    pr.BillingFrequency,
                    pr.ProductSpecification,
                    pr.Features,
                    pr.Rate,
                    pr.AmcCalculationType,
                    pr.AmcPercentage,
                    pr.AmcAmount,
                    pr.IsActive,
                    pr.CreatedAt,
                    ISNULL(dis.DiscountOfferCount, 0) AS DiscountOfferCount,
                    ISNULL(dis.ActiveDiscountOfferCount, 0) AS ActiveDiscountOfferCount
                FROM ProductRate pr
                INNER JOIN ProductMaster p ON p.Id = pr.ProductId
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS DiscountOfferCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveDiscountOfferCount
                    FROM ProductRateDiscountOffer d
                    WHERE d.ProductRateId = pr.Id
                ) dis
                WHERE pr.Id = @Id", new { Id = id });
        }

        public async Task<bool> RateVariantExistsAsync(int productId, string pricingModel, string billingModel, string billingFrequency, int? ignoreId = null)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM ProductRate
                    WHERE ProductId = @ProductId
                      AND PricingModel = @PricingModel
                      AND BillingModel = @BillingModel
                      AND ISNULL(BillingFrequency, '') = @BillingFrequency
                      AND (@IgnoreId IS NULL OR Id <> @IgnoreId)
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                new
                {
                    ProductId = productId,
                    PricingModel = pricingModel,
                    BillingModel = billingModel,
                    BillingFrequency = billingFrequency,
                    IgnoreId = ignoreId
                });
        }

        public async Task<int> CreateAsync(ProductRate productRate)
        {
            using var conn = CreateConnection();
            productRate.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ProductRate
                    (ProductId, PricingModel, BillingModel, BillingFrequency, ProductSpecification, Features, Rate, AmcCalculationType, AmcPercentage, AmcAmount, IsActive, CreatedAt)
                VALUES
                    (@ProductId, @PricingModel, @BillingModel, @BillingFrequency, @ProductSpecification, @Features, @Rate, @AmcCalculationType, @AmcPercentage, @AmcAmount, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", productRate);
        }

        public async Task<bool> UpdateAsync(ProductRate productRate)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE ProductRate SET
                    ProductId = @ProductId,
                    PricingModel = @PricingModel,
                    BillingModel = @BillingModel,
                    BillingFrequency = @BillingFrequency,
                    ProductSpecification = @ProductSpecification,
                    Features = @Features,
                    Rate = @Rate,
                    AmcCalculationType = @AmcCalculationType,
                    AmcPercentage = @AmcPercentage,
                    AmcAmount = @AmcAmount,
                    IsActive = @IsActive
                WHERE Id = @Id", productRate) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM ProductRate WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
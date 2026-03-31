using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ProductRateDiscountRepository : IProductRateDiscountRepository
    {
        private readonly string _connectionString;

        public ProductRateDiscountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ProductRateDiscountOffer>> GetAllAsync(int? productRateId = null, bool todayOnly = false)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProductRateDiscountOffer>(@"
                SELECT
                    d.Id,
                    d.ProductRateId,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductType,
                    pr.PricingModel,
                    pr.BillingModel,
                    pr.BillingFrequency,
                    pr.Rate AS BaseRate,
                    d.DiscountName,
                    d.DiscountType,
                    d.DiscountValue,
                    d.PromoCode,
                    d.ValidFrom,
                    d.ValidTo,
                    d.Description,
                    d.IsActive,
                    d.CreatedAt
                FROM ProductRateDiscountOffer d
                INNER JOIN ProductRate pr ON pr.Id = d.ProductRateId
                INNER JOIN ProductMaster p ON p.Id = pr.ProductId
                WHERE (@ProductRateId IS NULL OR d.ProductRateId = @ProductRateId)
                  AND (
                      @TodayOnly = 0
                      OR (
                          d.IsActive = 1
                          AND d.ValidFrom <= CAST(GETDATE() AS DATE)
                          AND d.ValidTo >= CAST(GETDATE() AS DATE)
                      )
                  )
                ORDER BY
                    CASE
                        WHEN d.IsActive = 1 AND d.ValidTo < CAST(GETDATE() AS DATE) THEN 0
                        WHEN d.IsActive = 1 AND d.ValidTo = CAST(GETDATE() AS DATE) THEN 1
                        WHEN d.IsActive = 1 AND d.ValidTo <= DATEADD(DAY, 3, CAST(GETDATE() AS DATE)) THEN 2
                        WHEN d.IsActive = 1 AND d.ValidFrom > CAST(GETDATE() AS DATE) THEN 3
                        WHEN d.IsActive = 0 THEN 4
                        ELSE 5
                    END,
                    d.ValidTo,
                    d.CreatedAt DESC", new { ProductRateId = productRateId, TodayOnly = todayOnly });
        }

        public async Task<ProductRateDiscountOffer?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ProductRateDiscountOffer>(@"
                SELECT
                    d.Id,
                    d.ProductRateId,
                    p.ProductCode,
                    p.ProductName,
                    p.ProductType,
                    pr.PricingModel,
                    pr.BillingModel,
                    pr.BillingFrequency,
                    pr.Rate AS BaseRate,
                    d.DiscountName,
                    d.DiscountType,
                    d.DiscountValue,
                    d.PromoCode,
                    d.ValidFrom,
                    d.ValidTo,
                    d.Description,
                    d.IsActive,
                    d.CreatedAt
                FROM ProductRateDiscountOffer d
                INNER JOIN ProductRate pr ON pr.Id = d.ProductRateId
                INNER JOIN ProductMaster p ON p.Id = pr.ProductId
                WHERE d.Id = @Id", new { Id = id });
        }

        public async Task<bool> PromoCodeExistsAsync(string promoCode, int? ignoreId = null)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM ProductRateDiscountOffer
                    WHERE PromoCode = @PromoCode
                      AND (@IgnoreId IS NULL OR Id <> @IgnoreId)
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                new { PromoCode = promoCode, IgnoreId = ignoreId });
        }

        public async Task<int> CreateAsync(ProductRateDiscountOffer offer)
        {
            using var conn = CreateConnection();
            offer.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ProductRateDiscountOffer
                    (ProductRateId, DiscountName, DiscountType, DiscountValue, PromoCode, ValidFrom, ValidTo, Description, IsActive, CreatedAt)
                VALUES
                    (@ProductRateId, @DiscountName, @DiscountType, @DiscountValue, @PromoCode, @ValidFrom, @ValidTo, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", offer);
        }

        public async Task<bool> UpdateAsync(ProductRateDiscountOffer offer)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE ProductRateDiscountOffer SET
                    ProductRateId = @ProductRateId,
                    DiscountName = @DiscountName,
                    DiscountType = @DiscountType,
                    DiscountValue = @DiscountValue,
                    PromoCode = @PromoCode,
                    ValidFrom = @ValidFrom,
                    ValidTo = @ValidTo,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE Id = @Id", offer) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM ProductRateDiscountOffer WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
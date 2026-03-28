using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ProductMasterRepository : IProductMasterRepository
    {
        private readonly string _connectionString;

        public ProductMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ProductMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProductMaster>(@"
                SELECT
                    p.*,
                    ISNULL(pr.PricingModelCount, 0) AS PricingModelCount,
                    ISNULL(pr.ActivePricingModelCount, 0) AS ActivePricingModelCount,
                    pr.MinRate,
                    pr.MaxRate,
                    ISNULL(dis.DiscountOfferCount, 0) AS DiscountOfferCount,
                    ISNULL(dis.ActiveDiscountOfferCount, 0) AS ActiveDiscountOfferCount,
                    ISNULL(dis.ActivePromoCodeCount, 0) AS ActivePromoCodeCount,
                    ISNULL(dis.ExpiringSoonDiscountOfferCount, 0) AS ExpiringSoonDiscountOfferCount,
                    dis.NextDiscountExpiryDate
                FROM ProductMaster p
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS PricingModelCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActivePricingModelCount,
                        MIN(Rate) AS MinRate,
                        MAX(Rate) AS MaxRate
                    FROM ProductRate pr
                    WHERE pr.ProductId = p.Id
                ) pr
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS DiscountOfferCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidFrom <= CAST(GETDATE() AS DATE)
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                            THEN 1 ELSE 0 END) AS ActiveDiscountOfferCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidFrom <= CAST(GETDATE() AS DATE)
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                             AND d.PromoCode IS NOT NULL
                             AND LTRIM(RTRIM(d.PromoCode)) <> ''
                            THEN 1 ELSE 0 END) AS ActivePromoCodeCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                             AND d.ValidTo <= DATEADD(DAY, 3, CAST(GETDATE() AS DATE))
                            THEN 1 ELSE 0 END) AS ExpiringSoonDiscountOfferCount,
                        MIN(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                            THEN d.ValidTo END) AS NextDiscountExpiryDate
                    FROM ProductRate pr
                    INNER JOIN ProductRateDiscountOffer d ON d.ProductRateId = pr.Id
                    WHERE pr.ProductId = p.Id
                ) dis
                ORDER BY p.ProductName, p.ProductCode");
        }

        public async Task<IEnumerable<ProductMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProductMaster>(@"
                SELECT *
                FROM ProductMaster
                WHERE IsActive = 1
                ORDER BY ProductName, ProductCode");
        }

        public async Task<ProductMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ProductMaster>(@"
                SELECT
                    p.*,
                    ISNULL(pr.PricingModelCount, 0) AS PricingModelCount,
                    ISNULL(pr.ActivePricingModelCount, 0) AS ActivePricingModelCount,
                    pr.MinRate,
                    pr.MaxRate,
                    ISNULL(dis.DiscountOfferCount, 0) AS DiscountOfferCount,
                    ISNULL(dis.ActiveDiscountOfferCount, 0) AS ActiveDiscountOfferCount,
                    ISNULL(dis.ActivePromoCodeCount, 0) AS ActivePromoCodeCount,
                    ISNULL(dis.ExpiringSoonDiscountOfferCount, 0) AS ExpiringSoonDiscountOfferCount,
                    dis.NextDiscountExpiryDate
                FROM ProductMaster p
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS PricingModelCount,
                        SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActivePricingModelCount,
                        MIN(Rate) AS MinRate,
                        MAX(Rate) AS MaxRate
                    FROM ProductRate pr
                    WHERE pr.ProductId = p.Id
                ) pr
                OUTER APPLY (
                    SELECT
                        COUNT(1) AS DiscountOfferCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidFrom <= CAST(GETDATE() AS DATE)
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                            THEN 1 ELSE 0 END) AS ActiveDiscountOfferCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidFrom <= CAST(GETDATE() AS DATE)
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                             AND d.PromoCode IS NOT NULL
                             AND LTRIM(RTRIM(d.PromoCode)) <> ''
                            THEN 1 ELSE 0 END) AS ActivePromoCodeCount,
                        SUM(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                             AND d.ValidTo <= DATEADD(DAY, 3, CAST(GETDATE() AS DATE))
                            THEN 1 ELSE 0 END) AS ExpiringSoonDiscountOfferCount,
                        MIN(CASE
                            WHEN d.IsActive = 1
                             AND d.ValidTo >= CAST(GETDATE() AS DATE)
                            THEN d.ValidTo END) AS NextDiscountExpiryDate
                    FROM ProductRate pr
                    INNER JOIN ProductRateDiscountOffer d ON d.ProductRateId = pr.Id
                    WHERE pr.ProductId = p.Id
                ) dis
                WHERE p.Id = @Id", new { Id = id });
        }

        public async Task<bool> ProductCodeExistsAsync(string productCode, int? ignoreId = null)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM ProductMaster
                    WHERE ProductCode = @ProductCode
                      AND (@IgnoreId IS NULL OR Id <> @IgnoreId)
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                new { ProductCode = productCode, IgnoreId = ignoreId });
        }

        public async Task<int> CreateAsync(ProductMaster product)
        {
            using var conn = CreateConnection();
            product.CreatedAt = DateTime.Now;
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ProductMaster (ProductCode, ProductName, ProductType, IsActive, CreatedAt)
                VALUES (@ProductCode, @ProductName, @ProductType, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", product);
        }

        public async Task<bool> UpdateAsync(ProductMaster product)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE ProductMaster SET
                    ProductCode = @ProductCode,
                    ProductName = @ProductName,
                    ProductType = @ProductType,
                    IsActive = @IsActive
                WHERE Id = @Id", product) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rateCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM ProductRate WHERE ProductId = @Id", new { Id = id });

            if (rateCount == 0)
            {
                return (true, null);
            }

            return (false, $"This product cannot be deleted because {rateCount} pricing model record(s) are mapped to it. Remove the product rates first.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM ProductMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
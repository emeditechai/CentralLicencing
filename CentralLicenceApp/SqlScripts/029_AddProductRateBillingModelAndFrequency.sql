IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProductRate') AND name = 'BillingModel')
BEGIN
    ALTER TABLE dbo.ProductRate ADD BillingModel NVARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ProductRate') AND name = 'BillingFrequency')
BEGIN
    ALTER TABLE dbo.ProductRate ADD BillingFrequency NVARCHAR(20) NULL;
END;

EXEC(N'
UPDATE dbo.ProductRate
SET BillingModel = ''One Time''
WHERE NULLIF(LTRIM(RTRIM(BillingModel)), '''') IS NULL;

UPDATE dbo.ProductRate
SET BillingFrequency = ''''
WHERE BillingFrequency IS NULL;

ALTER TABLE dbo.ProductRate ALTER COLUMN BillingModel NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ProductRate ALTER COLUMN BillingFrequency NVARCHAR(20) NOT NULL;
');

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductRate_ProductId_PricingModel'
      AND object_id = OBJECT_ID('dbo.ProductRate')
)
BEGIN
    DROP INDEX UX_ProductRate_ProductId_PricingModel ON dbo.ProductRate;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductRate_ProductId_PricingModel_BillingModel_BillingFrequency'
      AND object_id = OBJECT_ID('dbo.ProductRate')
)
BEGIN
    CREATE UNIQUE INDEX UX_ProductRate_ProductId_PricingModel_BillingModel_BillingFrequency
        ON dbo.ProductRate(ProductId, PricingModel, BillingModel, BillingFrequency);
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'BillingModel')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD BillingModel NVARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'BillingFrequency')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD BillingFrequency NVARCHAR(20) NULL;
END;

EXEC(N'
UPDATE dbo.ClientPurchasedProduct
SET BillingModel = ''One Time''
WHERE NULLIF(LTRIM(RTRIM(BillingModel)), '''') IS NULL;

UPDATE dbo.ClientPurchasedProduct
SET BillingFrequency = ''''
WHERE BillingFrequency IS NULL;

ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN BillingModel NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN BillingFrequency NVARCHAR(20) NOT NULL;
');

IF OBJECT_ID('dbo.usp_Report_ClientDetails', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.usp_Report_ClientDetails;
END;
GO

CREATE PROCEDURE dbo.usp_Report_ClientDetails
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @ProductType NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cl.ClientCode,
        cl.ClientName,
        cl.ProductType,
        cl.ContactNumber,
        cl.EmailID,
        cd.ClientPersonName,
        cd.address AS Address,
        ISNULL(cd.IsInternalUse, 0) AS IsInternalUse,
        cd.ReferenceClientCode,
        COALESCE(pp.PurchasedProductSummary, NULLIF(LTRIM(RTRIM(cd.ProductPurchased)), ''), '') AS PurchasedProductSummary,
        cl.Startdate AS LicenseStartDate,
        COALESCE(cd.IsActive, cl.IsActive) AS IsActive
    FROM dbo.ClientAppLicense cl
    LEFT JOIN dbo.ClientDetails cd ON cd.ClientCode = cl.ClientCode
    OUTER APPLY (
        SELECT STRING_AGG(CONCAT(cpp.ProductName, ' - ', cpp.PricingModel, ' / ', cpp.BillingModel, CASE WHEN NULLIF(LTRIM(RTRIM(cpp.BillingFrequency)), '') IS NULL THEN '' ELSE ' / ' + cpp.BillingFrequency END, ' (Base: ₹', CONVERT(VARCHAR(30), CAST(cpp.BasePrice AS DECIMAL(18,2))), ', AMC: ₹', CONVERT(VARCHAR(30), CAST(cpp.AmcAmount AS DECIMAL(18,2))), ')'), ', ')
            AS PurchasedProductSummary
        FROM dbo.ClientPurchasedProduct cpp
        WHERE cpp.ClientCode = cl.ClientCode
    ) pp
    WHERE (@FromDate IS NULL OR CAST(cl.Startdate AS DATE) >= @FromDate)
      AND (@ToDate IS NULL OR CAST(cl.Startdate AS DATE) <= @ToDate)
      AND (@ProductType IS NULL OR LTRIM(RTRIM(@ProductType)) = '' OR cl.ProductType = @ProductType)
    ORDER BY cl.Startdate DESC, cl.ClientCode;
END;
GO
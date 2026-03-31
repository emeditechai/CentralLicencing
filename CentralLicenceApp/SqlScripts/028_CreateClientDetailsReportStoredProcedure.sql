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
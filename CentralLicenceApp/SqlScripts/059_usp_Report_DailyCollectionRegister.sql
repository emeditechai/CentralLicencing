-- ============================================================
-- Daily Collection Register Report
-- Shows invoice payments received, filterable by date range
-- and by collected-by user (CreatedBy username).
-- When @CollectedBy is NULL all users' payments are returned.
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.usp_Report_DailyCollectionRegister') AND type = 'P')
    DROP PROCEDURE dbo.usp_Report_DailyCollectionRegister;
GO

CREATE PROCEDURE dbo.usp_Report_DailyCollectionRegister
    @FromDate     DATE          = NULL,
    @ToDate       DATE          = NULL,
    @CollectedBy  NVARCHAR(256) = NULL,   -- NULL = all users (admin view)
    @Page         INT           = 1,
    @PageSize     INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;

    SELECT @TotalCount = COUNT(*)
    FROM   InvoicePayment ip
    WHERE  ip.IsVoided = 0
      AND  (@FromDate    IS NULL OR CAST(ip.PaymentDate AS DATE) >= @FromDate)
      AND  (@ToDate      IS NULL OR CAST(ip.PaymentDate AS DATE) <= @ToDate)
      AND  (@CollectedBy IS NULL OR ip.CreatedBy = @CollectedBy);

    SELECT ip.Id,
           ip.ReceiptNo,
           ip.PaymentDate,
           ip.InvoiceNo,
           ip.PartyName,
           ISNULL(pm.PaymentModes, '') AS PaymentModes,
           ip.TotalAmountPaid,
           ISNULL(ip.CreatedBy, '')    AS CollectedBy,
           fy.FYCode,
           @TotalCount                 AS TotalCount
    FROM   InvoicePayment ip
    LEFT JOIN FinancialYearMaster fy ON fy.Id = ip.FinancialYearId
    OUTER APPLY (
        SELECT STRING_AGG(
                   ISNULL(pl.PaymentModeName, 'N/A') + ' - Rs ' + FORMAT(pl.Amount, 'N2'),
                   ', '
               ) AS PaymentModes
        FROM   InvoicePaymentLine pl
        WHERE  pl.PaymentId = ip.Id
    ) pm
    WHERE  ip.IsVoided = 0
      AND  (@FromDate    IS NULL OR CAST(ip.PaymentDate AS DATE) >= @FromDate)
      AND  (@ToDate      IS NULL OR CAST(ip.PaymentDate AS DATE) <= @ToDate)
      AND  (@CollectedBy IS NULL OR ip.CreatedBy = @CollectedBy)
    ORDER BY ip.PaymentDate DESC, ip.Id DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

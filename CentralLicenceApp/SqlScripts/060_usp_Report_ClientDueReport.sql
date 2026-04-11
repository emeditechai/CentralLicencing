-- ============================================================
-- Client-wise Due Report
-- Shows outstanding invoices with balance due per party
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.usp_Report_ClientDueReport') AND type = 'P')
    DROP PROCEDURE dbo.usp_Report_ClientDueReport;
GO

CREATE PROCEDURE dbo.usp_Report_ClientDueReport
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL,
    @Page       INT  = 1,
    @PageSize   INT  = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;

    SELECT @TotalCount = COUNT(*)
    FROM   Invoice i
    WHERE  i.Status NOT IN ('Paid', 'Cancelled')
      AND  (i.TotalAmount - i.ReceivedAmount) > 0
      AND  (@FromDate IS NULL OR CAST(i.InvoiceDate AS DATE) >= @FromDate)
      AND  (@ToDate   IS NULL OR CAST(i.InvoiceDate AS DATE) <= @ToDate);

    SELECT i.Id,
           i.PartyName,
           pm.GSTINNo          AS PartyGSTINNo,
           i.InvoiceNo,
           i.InvoiceDate,
           i.DueDate,
           i.TotalAmount,
           i.ReceivedAmount,
           (i.TotalAmount - i.ReceivedAmount) AS BalanceDue,
           CASE
               WHEN i.DueDate IS NOT NULL AND CAST(GETDATE() AS DATE) > CAST(i.DueDate AS DATE)
               THEN DATEDIFF(DAY, i.DueDate, GETDATE())
               ELSE 0
           END                 AS OverdueDays,
           i.Status,
           fy.FYCode,
           @TotalCount         AS TotalCount
    FROM   Invoice i
    LEFT JOIN PartyMaster pm ON pm.Id = i.PartyId
    LEFT JOIN FinancialYearMaster fy ON fy.Id = i.FinancialYearId
    WHERE  i.Status NOT IN ('Paid', 'Cancelled')
      AND  (i.TotalAmount - i.ReceivedAmount) > 0
      AND  (@FromDate IS NULL OR CAST(i.InvoiceDate AS DATE) >= @FromDate)
      AND  (@ToDate   IS NULL OR CAST(i.InvoiceDate AS DATE) <= @ToDate)
    ORDER BY
        CASE
            WHEN i.DueDate IS NOT NULL AND CAST(GETDATE() AS DATE) > CAST(i.DueDate AS DATE)
            THEN DATEDIFF(DAY, i.DueDate, GETDATE())
            ELSE 0
        END DESC,
        i.PartyName ASC,
        i.InvoiceDate DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

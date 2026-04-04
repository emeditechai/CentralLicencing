-- ============================================================
-- usp_Report_ExpenseDetails
-- Returns expense requests filtered by request creation date.
-- @UserId = NULL  => returns all records (Administrator view)
-- @UserId = <id>  => returns only that employee's records
-- Supports server-side pagination via @Page / @PageSize.
-- Every row includes TotalCount (COUNT(*) OVER()) for the caller.
-- ============================================================
IF OBJECT_ID('dbo.usp_Report_ExpenseDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Report_ExpenseDetails;
GO

CREATE PROCEDURE dbo.usp_Report_ExpenseDetails
    @FromDate  DATE = NULL,
    @ToDate    DATE = NULL,
    @UserId    INT  = NULL,
    @Page      INT  = 1,
    @PageSize  INT  = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        er.Id,
        er.RequestNumber,
        emp.FullName        AS RequestedByUser,
        emp.EmployeeCode,
        er.PurposeOfTravel,
        er.Status,
        er.TotalAmount,
        er.ItemCount,
        er.CreatedAt,
        er.SubmittedAt,
        er.ApprovedAt,
        appr.FullName       AS ApprovedByUser,
        er.SettledAt,
        stlr.FullName       AS SettledByUser,
        er.SettlementAmount,
        COUNT(*) OVER()     AS TotalCount
    FROM dbo.ExpenseRequest er
    INNER JOIN dbo.UserMaster emp  ON emp.Id  = er.EmployeeId
    LEFT  JOIN dbo.UserMaster appr ON appr.Id = er.ApprovedById
    LEFT  JOIN dbo.UserMaster stlr ON stlr.Id = er.SettledById
    WHERE
        (@UserId IS NULL OR er.EmployeeId = @UserId)
        AND (@FromDate IS NULL OR CAST(er.CreatedAt AS DATE) >= @FromDate)
        AND (@ToDate   IS NULL OR CAST(er.CreatedAt AS DATE) <= @ToDate)
    ORDER BY er.CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- ============================================================
-- usp_Report_SettlementDetails
-- Returns only settled expense requests, filtered by SettlementDate.
-- @UserId = NULL  => returns all records (Administrator view)
-- @UserId = <id>  => returns only that employee's records
-- Supports server-side pagination via @Page / @PageSize.
-- Every row includes TotalCount (COUNT(*) OVER()) for the caller.
-- ============================================================
IF OBJECT_ID('dbo.usp_Report_SettlementDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Report_SettlementDetails;
GO

CREATE PROCEDURE dbo.usp_Report_SettlementDetails
    @FromDate  DATE = NULL,
    @ToDate    DATE = NULL,
    @UserId    INT  = NULL,
    @Page      INT  = 1,
    @PageSize  INT  = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        er.Id,
        er.RequestNumber,
        emp.FullName          AS RequestedByUser,
        emp.EmployeeCode,
        er.PurposeOfTravel,
        er.TotalAmount,
        er.SettlementAmount,
        er.SettlementDate,
        er.SettledAt,
        er.SettlementMode,
        er.SettlementReferenceNo,
        er.SettlementRemarks,
        er.SettlementReceiptNumber,
        stlr.FullName         AS SettledByUser,
        COUNT(*) OVER()       AS TotalCount
    FROM dbo.ExpenseRequest er
    INNER JOIN dbo.UserMaster emp  ON emp.Id  = er.EmployeeId
    LEFT  JOIN dbo.UserMaster stlr ON stlr.Id = er.SettledById
    WHERE
        er.Status = 'Settled'
        AND (@UserId IS NULL OR er.EmployeeId = @UserId)
        AND (@FromDate IS NULL OR er.SettlementDate >= @FromDate)
        AND (@ToDate   IS NULL OR er.SettlementDate <= @ToDate)
    ORDER BY er.SettlementDate DESC, er.SettledAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO

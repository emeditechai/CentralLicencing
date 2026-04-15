-- ============================================================
-- 069  Payout Report Stored Procedures
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_PayoutSummaryReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_PayoutSummaryReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_PayoutSummaryReport
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL,
    @UserId   INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id AS UserId,
        u.FullName AS UserName,
        u.EmployeeCode,
        ISNULL(pc.PayoutModel, 'N/A') AS PayoutModel,
        COUNT(b.Id) AS BatchCount,
        SUM(b.TotalTasks) AS TotalTasks,
        SUM(b.TotalMinutes) AS TotalMinutes,
        SUM(b.GrossAmount) AS TotalGrossAmount,
        SUM(b.DeductionAmount) AS TotalDeductions,
        SUM(b.NetAmount) AS TotalNetAmount,
        SUM(CASE WHEN b.Status = 'Paid' THEN ISNULL(b.SettlementAmount, 0) ELSE 0 END) AS TotalPaid,
        SUM(CASE WHEN b.Status <> 'Paid' AND b.Status <> 'Rejected' THEN b.NetAmount ELSE 0 END) AS TotalPending
    FROM UserMaster u
    INNER JOIN PayoutBatch b ON b.UserId = u.Id
    LEFT JOIN PayoutConfiguration pc ON pc.UserId = u.Id AND pc.IsActive = 1
    WHERE b.Status <> 'Rejected'
      AND (@FromDate IS NULL OR b.ToDate >= @FromDate)
      AND (@ToDate IS NULL OR b.FromDate <= @ToDate)
      AND (@UserId IS NULL OR u.Id = @UserId)
    GROUP BY u.Id, u.FullName, u.EmployeeCode, pc.PayoutModel
    ORDER BY u.FullName;
END
GO

-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_PayoutDetailReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_PayoutDetailReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_PayoutDetailReport
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL,
    @UserId   INT  = NULL,
    @BatchId  INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id AS BatchId,
        u.FullName AS UserName,
        CONVERT(VARCHAR, b.FromDate, 106) + ' – ' + CONVERT(VARCHAR, b.ToDate, 106) AS BatchPeriod,
        b.Status AS BatchStatus,
        bl.TaskId,
        bl.TaskTitle,
        bl.TaskTypeName,
        bl.TaskCategoryName,
        bl.ProjectModuleName,
        bl.TimeSpentMinutes,
        bl.RateApplied,
        bl.RateSource,
        bl.Amount,
        bl.TaskCompletedAt
    FROM PayoutBatchLine bl
    INNER JOIN PayoutBatch b ON b.Id = bl.BatchId
    INNER JOIN UserMaster u ON u.Id = b.UserId
    WHERE b.Status <> 'Rejected'
      AND (@FromDate IS NULL OR b.ToDate >= @FromDate)
      AND (@ToDate IS NULL OR b.FromDate <= @ToDate)
      AND (@UserId IS NULL OR b.UserId = @UserId)
      AND (@BatchId IS NULL OR b.Id = @BatchId)
    ORDER BY u.FullName, b.FromDate, bl.TaskCompletedAt;
END
GO

-- ────────────────────────────────────────────────────────────

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_PayoutHistoryReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_PayoutHistoryReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_PayoutHistoryReport
    @FromDate DATE       = NULL,
    @ToDate   DATE       = NULL,
    @UserId   INT        = NULL,
    @Status   NVARCHAR(30) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Id AS BatchId,
        b.UserId,
        u.FullName AS UserName,
        u.EmployeeCode,
        b.FromDate,
        b.ToDate,
        b.PayoutModel,
        b.TotalTasks,
        b.TotalMinutes,
        b.GrossAmount,
        b.DeductionAmount,
        b.NetAmount,
        b.Status,
        b.GeneratedAt,
        g.FullName AS GeneratedByName,
        b.SettlementAmount,
        b.SettlementDate,
        b.SettledAt,
        s.FullName AS SettledByName,
        b.SettlementMode,
        b.SettlementReferenceNo,
        bk.BankName
    FROM PayoutBatch b
    INNER JOIN UserMaster u ON u.Id = b.UserId
    INNER JOIN UserMaster g ON g.Id = b.GeneratedById
    LEFT JOIN UserMaster s ON s.Id = b.SettledById
    LEFT JOIN BankMaster bk ON bk.Id = b.SettlementBankId
    WHERE (@FromDate IS NULL OR b.ToDate >= @FromDate)
      AND (@ToDate IS NULL OR b.FromDate <= @ToDate)
      AND (@UserId IS NULL OR b.UserId = @UserId)
      AND (@Status IS NULL OR b.Status = @Status)
    ORDER BY b.GeneratedAt DESC;
END
GO

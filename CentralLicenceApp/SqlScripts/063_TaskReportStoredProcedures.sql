-- ============================================================
-- 063 – Task Report Stored Procedures
-- ============================================================

-- ─────────────────────────────────────────────────────────────
-- 1) Timesheet Report
--    Date-wise time entries with task details
--    @UserId NULL = all users (admin), otherwise filter
-- ─────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.usp_Report_Timesheet', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Report_Timesheet;
GO

CREATE PROCEDURE dbo.usp_Report_Timesheet
    @FromDate   DATE          = NULL,
    @ToDate     DATE          = NULL,
    @UserId     INT           = NULL,
    @TaskTypeId INT           = NULL,
    @Page       INT           = 1,
    @PageSize   INT           = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalCount INT;

    SELECT @TotalCount = COUNT(*)
    FROM   TaskTimeLog tl
    INNER JOIN DailyTaskLog t  ON t.Id  = tl.TaskId
    WHERE  (@FromDate IS NULL OR tl.LogDate >= @FromDate)
      AND  (@ToDate   IS NULL OR tl.LogDate <= @ToDate)
      AND  (@UserId   IS NULL OR tl.UserId  = @UserId)
      AND  (@TaskTypeId IS NULL OR t.TaskTypeId = @TaskTypeId)
      AND  t.Status <> 'Cancelled';

    SELECT
        tl.Id,
        tl.LogDate,
        u.FullName          AS UserName,
        t.TaskTitle,
        tt.Name             AS TaskTypeName,
        tc.Name             AS TaskCategoryName,
        t.Status            AS TaskStatus,
        tl.TimeSpentMinutes,
        tl.Remarks,
        ISNULL(pm.Name, '') AS ProjectModuleName,
        ISNULL(tk.TicketNumber, '') AS TicketNumber,
        @TotalCount         AS TotalCount
    FROM   TaskTimeLog tl
    INNER JOIN DailyTaskLog t    ON t.Id   = tl.TaskId
    INNER JOIN UserMaster u      ON u.Id   = tl.UserId
    INNER JOIN TaskTypeMaster tt ON tt.Id  = t.TaskTypeId
    INNER JOIN TaskCategoryMaster tc ON tc.Id = t.TaskCategoryId
    LEFT  JOIN ProjectModuleMaster pm ON pm.Id = t.ProjectModuleId
    LEFT  JOIN HelpDeskTicket tk     ON tk.Id  = t.TicketId
    WHERE  (@FromDate IS NULL OR tl.LogDate >= @FromDate)
      AND  (@ToDate   IS NULL OR tl.LogDate <= @ToDate)
      AND  (@UserId   IS NULL OR tl.UserId  = @UserId)
      AND  (@TaskTypeId IS NULL OR t.TaskTypeId = @TaskTypeId)
      AND  t.Status <> 'Cancelled'
    ORDER BY tl.LogDate DESC, tl.Id DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ─────────────────────────────────────────────────────────────
-- 2) Employee Productivity Report
--    Per-user summary: tasks, hours, completion %, Dev vs Support
-- ─────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.usp_Report_EmployeeProductivity', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Report_EmployeeProductivity;
GO

CREATE PROCEDURE dbo.usp_Report_EmployeeProductivity
    @FromDate   DATE = NULL,
    @ToDate     DATE = NULL,
    @UserId     INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.UserId,
        u.FullName                     AS UserName,
        COUNT(DISTINCT t.Id)           AS TotalTasks,

        SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
        SUM(CASE WHEN t.Status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledTasks,
        SUM(CASE WHEN t.Status NOT IN ('Completed','Cancelled') THEN 1 ELSE 0 END) AS PendingTasks,

        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN tlog.TotalTime ELSE 0 END), 0) AS TotalMinutes,
        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Development' THEN tlog.TotalTime ELSE 0 END), 0) AS DevMinutes,
        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Support'     THEN tlog.TotalTime ELSE 0 END), 0) AS SupportMinutes,

        CASE WHEN COUNT(DISTINCT t.Id) > 0
             THEN CAST(SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) * 100.0 / COUNT(DISTINCT t.Id) AS DECIMAL(5,1))
             ELSE 0 END AS CompletionRate,

        CASE WHEN COUNT(DISTINCT CASE WHEN t.Status <> 'Cancelled' THEN t.Id END) > 0
             THEN ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN tlog.TotalTime ELSE 0 END), 0)
                  / COUNT(DISTINCT CASE WHEN t.Status <> 'Cancelled' THEN t.Id END)
             ELSE 0 END AS AvgMinutesPerTask

    FROM DailyTaskLog t
    INNER JOIN UserMaster u ON u.Id = t.UserId
    INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
    OUTER APPLY (
        SELECT SUM(TimeSpentMinutes) AS TotalTime
        FROM   TaskTimeLog
        WHERE  TaskId = t.Id
    ) tlog
    WHERE (@FromDate IS NULL OR t.TaskDate >= @FromDate)
      AND (@ToDate   IS NULL OR t.TaskDate <= @ToDate)
      AND (@UserId   IS NULL OR t.UserId   = @UserId)
    GROUP BY t.UserId, u.FullName
    ORDER BY TotalMinutes DESC;
END
GO

-- ─────────────────────────────────────────────────────────────
-- 3) Project / Module Wise Effort Report
--    Per-project summary with user breakdown
-- ─────────────────────────────────────────────────────────────
IF OBJECT_ID('dbo.usp_Report_ProjectEffort', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_Report_ProjectEffort;
GO

CREATE PROCEDURE dbo.usp_Report_ProjectEffort
    @FromDate       DATE = NULL,
    @ToDate         DATE = NULL,
    @UserId         INT  = NULL,
    @ProjectModuleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(pm.Id, 0)      AS ProjectModuleId,
        ISNULL(pm.Name, 'No Project / Unassigned') AS ProjectModuleName,
        u.Id                   AS UserId,
        u.FullName             AS UserName,
        COUNT(DISTINCT t.Id)   AS TaskCount,

        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN tlog.TotalTime ELSE 0 END), 0) AS TotalMinutes,
        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Development' THEN tlog.TotalTime ELSE 0 END), 0) AS DevMinutes,
        ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Support'     THEN tlog.TotalTime ELSE 0 END), 0) AS SupportMinutes,

        SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
        SUM(CASE WHEN t.Status = 'Cancelled' THEN 1 ELSE 0 END) AS CancelledTasks

    FROM DailyTaskLog t
    INNER JOIN UserMaster u ON u.Id = t.UserId
    INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
    LEFT  JOIN ProjectModuleMaster pm ON pm.Id = t.ProjectModuleId
    OUTER APPLY (
        SELECT SUM(TimeSpentMinutes) AS TotalTime
        FROM   TaskTimeLog
        WHERE  TaskId = t.Id
    ) tlog
    WHERE (@FromDate IS NULL OR t.TaskDate >= @FromDate)
      AND (@ToDate   IS NULL OR t.TaskDate <= @ToDate)
      AND (@UserId   IS NULL OR t.UserId   = @UserId)
      AND (@ProjectModuleId IS NULL OR ISNULL(t.ProjectModuleId, 0) = @ProjectModuleId)
    GROUP BY ISNULL(pm.Id, 0), ISNULL(pm.Name, 'No Project / Unassigned'), u.Id, u.FullName
    ORDER BY ISNULL(pm.Name, 'No Project / Unassigned'), TotalMinutes DESC;
END
GO

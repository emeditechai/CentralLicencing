-- ============================================================
-- Ticket Reporting & Analytics Stored Procedures
-- ============================================================

-- ── 1. Dashboard Summary KPIs ──
IF OBJECT_ID('dbo.usp_TicketReport_Dashboard', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_Dashboard;
GO
CREATE PROCEDURE dbo.usp_TicketReport_Dashboard
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*)                                                         AS TotalTickets,
        SUM(CASE WHEN t.Status = 'Open'               THEN 1 ELSE 0 END) AS OpenTickets,
        SUM(CASE WHEN t.Status = 'In Progress'         THEN 1 ELSE 0 END) AS InProgressTickets,
        SUM(CASE WHEN t.Status = 'Resolved'            THEN 1 ELSE 0 END) AS ResolvedTickets,
        SUM(CASE WHEN t.Status = 'Closed'              THEN 1 ELSE 0 END) AS ClosedTickets,
        SUM(CASE WHEN t.Status = 'Waiting for Client'  THEN 1 ELSE 0 END) AS WaitingTickets,
        ISNULL(AVG(
            CASE WHEN t.FirstResponseAt IS NOT NULL
                 THEN DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0
            END), 0)                                                     AS AvgResponseTimeHours,
        ISNULL(AVG(
            CASE WHEN t.ResolvedAt IS NOT NULL
                 THEN DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0
            END), 0)                                                     AS AvgResolutionTimeHours
    FROM HelpDeskTicket t
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate);
END
GO

-- ── 2. Status Distribution ──
IF OBJECT_ID('dbo.usp_TicketReport_StatusDistribution', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_StatusDistribution;
GO
CREATE PROCEDURE dbo.usp_TicketReport_StatusDistribution
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT t.Status, COUNT(*) AS [Count]
    FROM HelpDeskTicket t
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
    GROUP BY t.Status
    ORDER BY [Count] DESC;
END
GO

-- ── 3. Category Distribution ──
IF OBJECT_ID('dbo.usp_TicketReport_CategoryDistribution', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_CategoryDistribution;
GO
CREATE PROCEDURE dbo.usp_TicketReport_CategoryDistribution
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.CategoryName, COUNT(*) AS [Count]
    FROM HelpDeskTicket t
    INNER JOIN TicketCategoryMaster c ON c.Id = t.CategoryId
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
    GROUP BY c.CategoryName
    ORDER BY [Count] DESC;
END
GO

-- ── 4. Priority Distribution ──
IF OBJECT_ID('dbo.usp_TicketReport_PriorityDistribution', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_PriorityDistribution;
GO
CREATE PROCEDURE dbo.usp_TicketReport_PriorityDistribution
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT p.PriorityName, p.ColorCode, COUNT(*) AS [Count]
    FROM HelpDeskTicket t
    INNER JOIN TicketPriorityMaster p ON p.Id = t.PriorityId
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
    GROUP BY p.PriorityName, p.ColorCode
    ORDER BY p.ColorCode DESC;
END
GO

-- ── 5. Daily Trend (created vs resolved per day) ──
IF OBJECT_ID('dbo.usp_TicketReport_DailyTrend', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_DailyTrend;
GO
CREATE PROCEDURE dbo.usp_TicketReport_DailyTrend
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH DateRange AS (
        SELECT ISNULL(@FromDate, CAST(DATEADD(DAY, -30, GETDATE()) AS DATE)) AS dt
        UNION ALL
        SELECT DATEADD(DAY, 1, dt) FROM DateRange
        WHERE dt < ISNULL(@ToDate, CAST(GETDATE() AS DATE))
    ),
    Created AS (
        SELECT CAST(t.CreatedAt AS DATE) AS TicketDate, COUNT(*) AS CreatedCount
        FROM HelpDeskTicket t
        WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
          AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
        GROUP BY CAST(t.CreatedAt AS DATE)
    ),
    Resolved AS (
        SELECT CAST(t.ResolvedAt AS DATE) AS TicketDate, COUNT(*) AS ResolvedCount
        FROM HelpDeskTicket t
        WHERE t.ResolvedAt IS NOT NULL
          AND (@FromDate IS NULL OR CAST(t.ResolvedAt AS DATE) >= @FromDate)
          AND (@ToDate   IS NULL OR CAST(t.ResolvedAt AS DATE) <= @ToDate)
        GROUP BY CAST(t.ResolvedAt AS DATE)
    )
    SELECT d.dt AS TicketDate,
           ISNULL(c.CreatedCount, 0) AS CreatedCount,
           ISNULL(r.ResolvedCount, 0) AS ResolvedCount
    FROM DateRange d
    LEFT JOIN Created c ON c.TicketDate = d.dt
    LEFT JOIN Resolved r ON r.TicketDate = d.dt
    ORDER BY d.dt
    OPTION (MAXRECURSION 366);
END
GO

-- ── 6. Agent Performance (paginated) ──
IF OBJECT_ID('dbo.usp_TicketReport_AgentPerformance', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_AgentPerformance;
GO
CREATE PROCEDURE dbo.usp_TicketReport_AgentPerformance
    @FromDate  DATE = NULL,
    @ToDate    DATE = NULL,
    @Page      INT  = 1,
    @PageSize  INT  = 20,
    @AgentId   INT  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id                                                              AS AgentId,
        ISNULL(u.FullName, u.Username)                                    AS AgentName,
        COUNT(*)                                                          AS TotalAssigned,
        SUM(CASE WHEN t.Status = 'Resolved'     THEN 1 ELSE 0 END)       AS Resolved,
        SUM(CASE WHEN t.Status = 'Closed'        THEN 1 ELSE 0 END)      AS Closed,
        SUM(CASE WHEN t.Status = 'Open'           THEN 1 ELSE 0 END)     AS [Open],
        SUM(CASE WHEN t.Status = 'In Progress'    THEN 1 ELSE 0 END)     AS InProgress,
        ISNULL(AVG(
            CASE WHEN t.FirstResponseAt IS NOT NULL
                 THEN DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0
            END), 0)                                                      AS AvgResponseTimeHours,
        ISNULL(AVG(
            CASE WHEN t.ResolvedAt IS NOT NULL
                 THEN DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0
            END), 0)                                                      AS AvgResolutionTimeHours,
        CASE WHEN COUNT(*) > 0
             THEN CAST(SUM(CASE WHEN t.Status IN ('Resolved','Closed') THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(5,1))
             ELSE 0 END                                                   AS ResolutionRate,
        COUNT(*) OVER()                                                   AS TotalCount
    FROM HelpDeskTicket t
    INNER JOIN UserMaster u ON u.Id = t.AssignedToId
    WHERE t.AssignedToId IS NOT NULL
      AND (@AgentId IS NULL OR t.AssignedToId = @AgentId)
      AND (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
    GROUP BY u.Id, u.FullName, u.Username
    ORDER BY TotalAssigned DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ── 7. SLA Compliance (paginated) ──
IF OBJECT_ID('dbo.usp_TicketReport_SlaCompliance', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_SlaCompliance;
GO
CREATE PROCEDURE dbo.usp_TicketReport_SlaCompliance
    @FromDate  DATE = NULL,
    @ToDate    DATE = NULL,
    @Page      INT  = 1,
    @PageSize  INT  = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        t.Id              AS TicketId,
        t.TicketNumber,
        t.Subject,
        p.PriorityName,
        p.ColorCode       AS PriorityColor,
        t.Status,
        cr.FullName       AS CreatedByName,
        ag.FullName       AS AssignedToName,
        t.CreatedAt,
        t.FirstResponseAt,
        t.ResolvedAt,
        ISNULL(p.SlaResponseHours, 0)                                   AS SlaResponseHours,
        ISNULL(p.SlaResolutionHours, 0)                                  AS SlaResolutionHours,
        CASE WHEN t.FirstResponseAt IS NOT NULL
             THEN DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0
        END                                                               AS ActualResponseHours,
        CASE WHEN t.ResolvedAt IS NOT NULL
             THEN DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0
        END                                                               AS ActualResolutionHours,
        CASE
            WHEN t.FirstResponseAt IS NULL THEN 'Pending'
            WHEN DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0 <= ISNULL(p.SlaResponseHours, 9999) THEN 'Met'
            ELSE 'Breached'
        END                                                               AS ResponseSlaStatus,
        CASE
            WHEN t.ResolvedAt IS NULL AND t.Status NOT IN ('Resolved','Closed') THEN 'Pending'
            WHEN t.ResolvedAt IS NOT NULL AND DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0 <= ISNULL(p.SlaResolutionHours, 9999) THEN 'Met'
            WHEN t.ResolvedAt IS NOT NULL THEN 'Breached'
            ELSE 'Pending'
        END                                                               AS ResolutionSlaStatus,
        COUNT(*) OVER()                                                   AS TotalCount
    FROM HelpDeskTicket t
    INNER JOIN TicketPriorityMaster p ON p.Id = t.PriorityId
    INNER JOIN UserMaster cr ON cr.Id = t.CreatedById
    LEFT  JOIN UserMaster ag ON ag.Id = t.AssignedToId
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate)
    ORDER BY t.CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
END
GO

-- ── 8. SLA Compliance Summary (totals for cards - not paginated) ──
IF OBJECT_ID('dbo.usp_TicketReport_SlaComplianceSummary', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_TicketReport_SlaComplianceSummary;
GO
CREATE PROCEDURE dbo.usp_TicketReport_SlaComplianceSummary
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalTickets,
        SUM(CASE WHEN t.FirstResponseAt IS NOT NULL
                 AND DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0 <= ISNULL(p.SlaResponseHours, 9999)
                 THEN 1 ELSE 0 END) AS ResponseSlaMetCount,
        SUM(CASE WHEN t.FirstResponseAt IS NOT NULL
                 AND DATEDIFF(MINUTE, t.CreatedAt, t.FirstResponseAt) / 60.0 > ISNULL(p.SlaResponseHours, 9999)
                 THEN 1 ELSE 0 END) AS ResponseSlaBreachedCount,
        SUM(CASE WHEN t.ResolvedAt IS NOT NULL
                 AND DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0 <= ISNULL(p.SlaResolutionHours, 9999)
                 THEN 1 ELSE 0 END) AS ResolutionSlaMetCount,
        SUM(CASE WHEN t.ResolvedAt IS NOT NULL
                 AND DATEDIFF(MINUTE, t.CreatedAt, t.ResolvedAt) / 60.0 > ISNULL(p.SlaResolutionHours, 9999)
                 THEN 1 ELSE 0 END) AS ResolutionSlaBreachedCount
    FROM HelpDeskTicket t
    INNER JOIN TicketPriorityMaster p ON p.Id = t.PriorityId
    WHERE (@FromDate IS NULL OR CAST(t.CreatedAt AS DATE) >= @FromDate)
      AND (@ToDate   IS NULL OR CAST(t.CreatedAt AS DATE) <= @ToDate);
END
GO

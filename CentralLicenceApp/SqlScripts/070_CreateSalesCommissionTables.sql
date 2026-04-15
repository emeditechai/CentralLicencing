-- 070: Create Sales Commission tables & report stored procedures
-- =================================================================

-- 1) SalesCommissionConfiguration — one row per sales user
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesCommissionConfiguration')
BEGIN
    CREATE TABLE [dbo].[SalesCommissionConfiguration] (
        [Id]            INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]        INT             NOT NULL,
        [CommissionType] NVARCHAR(20)   NOT NULL,  -- 'Percentage' or 'FixedAmount'
        [DefaultRate]   DECIMAL(18,2)   NOT NULL,  -- % value or flat amount
        [EffectiveFrom] DATE            NOT NULL,
        [IsActive]      BIT             NOT NULL DEFAULT 1,
        [CreatedById]   INT             NOT NULL,
        [CreatedAt]     DATETIME2       NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]     DATETIME2       NULL,
        CONSTRAINT FK_SalesCommConfig_User       FOREIGN KEY ([UserId])      REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesCommConfig_CreatedBy  FOREIGN KEY ([CreatedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT CK_SalesCommConfig_Type       CHECK ([CommissionType] IN ('Percentage','FixedAmount')),
        CONSTRAINT UQ_SalesCommConfig_UserId     UNIQUE ([UserId])
    );

    CREATE NONCLUSTERED INDEX IX_SalesCommConfig_UserId
        ON [dbo].[SalesCommissionConfiguration] ([UserId]) WHERE [IsActive] = 1;
END
GO

-- 2) SalesCommissionRule — product-specific overrides (0..N per user)
--    Priority: ProductId=10, Default=0
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesCommissionRule')
BEGIN
    CREATE TABLE [dbo].[SalesCommissionRule] (
        [Id]             INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]         INT             NOT NULL,
        [ProductId]      INT             NULL,
        [CommissionType] NVARCHAR(20)    NOT NULL,  -- 'Percentage' or 'FixedAmount'
        [Rate]           DECIMAL(18,2)   NOT NULL,
        [Priority]       INT             NOT NULL DEFAULT 0,  -- 10=Product, 0=Default
        [EffectiveFrom]  DATE            NOT NULL,
        [IsActive]       BIT             NOT NULL DEFAULT 1,
        [CreatedById]    INT             NOT NULL,
        [CreatedAt]      DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SalesCommRule_User       FOREIGN KEY ([UserId])      REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesCommRule_Product    FOREIGN KEY ([ProductId])   REFERENCES [ProductMaster]([Id]),
        CONSTRAINT FK_SalesCommRule_CreatedBy  FOREIGN KEY ([CreatedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT CK_SalesCommRule_Type       CHECK ([CommissionType] IN ('Percentage','FixedAmount'))
    );

    CREATE NONCLUSTERED INDEX IX_SalesCommRule_UserId
        ON [dbo].[SalesCommissionRule] ([UserId], [IsActive]) INCLUDE ([Priority], [Rate]);
END
GO

-- 3) SalesInvoiceAssignment — maps Invoice → Sales Person
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesInvoiceAssignment')
BEGIN
    CREATE TABLE [dbo].[SalesInvoiceAssignment] (
        [Id]            INT         IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [InvoiceId]     INT         NOT NULL,
        [SalesUserId]   INT         NOT NULL,
        [ProductId]     INT         NULL,          -- optional, for product-rule matching
        [AssignedById]  INT         NOT NULL,
        [AssignedAt]    DATETIME2   NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SalesInvAssign_Invoice     FOREIGN KEY ([InvoiceId])    REFERENCES [Invoice]([Id]),
        CONSTRAINT FK_SalesInvAssign_SalesUser   FOREIGN KEY ([SalesUserId])  REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesInvAssign_Product     FOREIGN KEY ([ProductId])    REFERENCES [ProductMaster]([Id]),
        CONSTRAINT FK_SalesInvAssign_AssignedBy  FOREIGN KEY ([AssignedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT UQ_SalesInvAssign_InvoiceId   UNIQUE ([InvoiceId])
    );

    CREATE NONCLUSTERED INDEX IX_SalesInvAssign_SalesUserId
        ON [dbo].[SalesInvoiceAssignment] ([SalesUserId]);
END
GO

-- 4) SalesCommissionBatch — one per sales user per generated period
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesCommissionBatch')
BEGIN
    CREATE TABLE [dbo].[SalesCommissionBatch] (
        [Id]                      INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]                  INT             NOT NULL,
        [FromDate]                DATE            NOT NULL,
        [ToDate]                  DATE            NOT NULL,
        [CommissionTypeSnapshot]  NVARCHAR(20)    NOT NULL,   -- snapshot at generation
        [DefaultRateSnapshot]     DECIMAL(18,2)   NOT NULL,
        [TotalInvoices]           INT             NOT NULL DEFAULT 0,
        [TotalPayments]           INT             NOT NULL DEFAULT 0,
        [TotalSalesAmount]        DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [GrossCommission]         DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [DeductionAmount]         DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [NetCommission]           DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [Status]                  NVARCHAR(30)    NOT NULL DEFAULT 'Draft',
        [Remarks]                 NVARCHAR(500)   NULL,
        [GeneratedAt]             DATETIME2       NOT NULL DEFAULT GETDATE(),
        [GeneratedById]           INT             NOT NULL,
        -- Settlement fields
        [SettlementAmount]        DECIMAL(18,2)   NULL,
        [SettlementDate]          DATE            NULL,
        [SettledAt]               DATETIME2       NULL,
        [SettledById]             INT             NULL,
        [SettlementMode]          NVARCHAR(50)    NULL,
        [SettlementReferenceNo]   NVARCHAR(100)   NULL,
        [SettlementBankId]        INT             NULL,
        [SettlementRemarks]       NVARCHAR(500)   NULL,
        CONSTRAINT FK_SalesCommBatch_User         FOREIGN KEY ([UserId])           REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesCommBatch_GeneratedBy  FOREIGN KEY ([GeneratedById])    REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesCommBatch_SettledBy    FOREIGN KEY ([SettledById])      REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_SalesCommBatch_Bank         FOREIGN KEY ([SettlementBankId]) REFERENCES [BankMaster]([Id]),
        CONSTRAINT CK_SalesCommBatch_Status       CHECK ([Status] IN ('Draft','PendingApproval','L1Approved','Approved','Paid','Rejected')),
        CONSTRAINT CK_SalesCommBatch_CommType     CHECK ([CommissionTypeSnapshot] IN ('Percentage','FixedAmount'))
    );

    CREATE NONCLUSTERED INDEX IX_SalesCommBatch_UserId_Status
        ON [dbo].[SalesCommissionBatch] ([UserId], [Status]) INCLUDE ([FromDate], [ToDate], [NetCommission]);
    CREATE NONCLUSTERED INDEX IX_SalesCommBatch_Status
        ON [dbo].[SalesCommissionBatch] ([Status]) INCLUDE ([UserId], [FromDate], [ToDate]);
END
GO

-- 5) SalesCommissionBatchLine — one per eligible payment in batch
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesCommissionBatchLine')
BEGIN
    CREATE TABLE [dbo].[SalesCommissionBatchLine] (
        [Id]                INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BatchId]           INT             NOT NULL,
        [InvoicePaymentId]  INT             NOT NULL,
        [InvoiceId]         INT             NOT NULL,
        [InvoiceNo]         VARCHAR(30)     NOT NULL,       -- snapshot
        [PartyName]         NVARCHAR(150)   NOT NULL,       -- snapshot
        [PaymentDate]       DATE            NOT NULL,
        [PaymentAmount]     DECIMAL(18,2)   NOT NULL,
        [ProductId]         INT             NULL,
        [ProductName]       NVARCHAR(200)   NULL,           -- snapshot
        [CommissionType]    NVARCHAR(20)    NOT NULL,       -- 'Percentage' or 'FixedAmount'
        [RateApplied]       DECIMAL(18,2)   NOT NULL,
        [RateSource]        NVARCHAR(30)    NOT NULL,       -- 'Product' or 'Default'
        [CommissionAmount]  DECIMAL(18,2)   NOT NULL,
        CONSTRAINT FK_SalesCommBatchLine_Batch     FOREIGN KEY ([BatchId])          REFERENCES [SalesCommissionBatch]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_SalesCommBatchLine_Payment   FOREIGN KEY ([InvoicePaymentId]) REFERENCES [InvoicePayment]([Id]),
        CONSTRAINT FK_SalesCommBatchLine_Invoice   FOREIGN KEY ([InvoiceId])        REFERENCES [Invoice]([Id]),
        CONSTRAINT CK_SalesCommBatchLine_CommType  CHECK ([CommissionType] IN ('Percentage','FixedAmount')),
        CONSTRAINT CK_SalesCommBatchLine_Source    CHECK ([RateSource] IN ('Product','Default'))
    );

    CREATE NONCLUSTERED INDEX IX_SalesCommBatchLine_BatchId
        ON [dbo].[SalesCommissionBatchLine] ([BatchId]);
    CREATE UNIQUE NONCLUSTERED INDEX UX_SalesCommBatchLine_PaymentId
        ON [dbo].[SalesCommissionBatchLine] ([InvoicePaymentId]);
END
GO

-- 6) SalesCommissionApprovalHistory — approval/rejection trail
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SalesCommissionApprovalHistory')
BEGIN
    CREATE TABLE [dbo].[SalesCommissionApprovalHistory] (
        [Id]            INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BatchId]       INT             NOT NULL,
        [ApproverLevel] INT             NOT NULL,  -- 1 = Core Member, 2 = Admin/Finance
        [ApprovedById]  INT             NOT NULL,
        [Status]        NVARCHAR(20)    NOT NULL,  -- 'Approved' or 'Rejected'
        [Remarks]       NVARCHAR(500)   NULL,
        [ApprovedAt]    DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_SalesCommApproval_Batch    FOREIGN KEY ([BatchId])      REFERENCES [SalesCommissionBatch]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_SalesCommApproval_Approver FOREIGN KEY ([ApprovedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT CK_SalesCommApproval_Status   CHECK ([Status] IN ('Approved','Rejected'))
    );

    CREATE NONCLUSTERED INDEX IX_SalesCommApproval_BatchId
        ON [dbo].[SalesCommissionApprovalHistory] ([BatchId]);
END
GO

-- ════════════════════════════════════════════════════════════════
-- REPORT STORED PROCEDURES
-- ════════════════════════════════════════════════════════════════

-- ── Summary Report ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_SalesCommissionSummaryReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_SalesCommissionSummaryReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_SalesCommissionSummaryReport
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
        ISNULL(sc.CommissionType, 'N/A') AS CommissionType,
        COUNT(b.Id) AS BatchCount,
        SUM(b.TotalPayments) AS TotalPayments,
        SUM(b.TotalSalesAmount) AS TotalSalesAmount,
        SUM(b.GrossCommission) AS TotalGrossCommission,
        SUM(b.DeductionAmount) AS TotalDeductions,
        SUM(b.NetCommission) AS TotalNetCommission,
        SUM(CASE WHEN b.Status = 'Paid' THEN ISNULL(b.SettlementAmount, 0) ELSE 0 END) AS TotalPaid,
        SUM(CASE WHEN b.Status <> 'Paid' AND b.Status <> 'Rejected' THEN b.NetCommission ELSE 0 END) AS TotalPending
    FROM UserMaster u
    INNER JOIN SalesCommissionBatch b ON b.UserId = u.Id
    LEFT JOIN SalesCommissionConfiguration sc ON sc.UserId = u.Id AND sc.IsActive = 1
    WHERE b.Status <> 'Rejected'
      AND (@FromDate IS NULL OR b.ToDate >= @FromDate)
      AND (@ToDate IS NULL OR b.FromDate <= @ToDate)
      AND (@UserId IS NULL OR u.Id = @UserId)
    GROUP BY u.Id, u.FullName, u.EmployeeCode, sc.CommissionType
    ORDER BY u.FullName;
END
GO

-- ── Detail Report ──────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_SalesCommissionDetailReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_SalesCommissionDetailReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_SalesCommissionDetailReport
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
        CONVERT(VARCHAR, b.FromDate, 106) + N' – ' + CONVERT(VARCHAR, b.ToDate, 106) AS BatchPeriod,
        b.Status AS BatchStatus,
        bl.InvoiceNo,
        bl.PartyName,
        bl.PaymentDate,
        bl.PaymentAmount,
        bl.ProductName,
        bl.CommissionType,
        bl.RateApplied,
        bl.RateSource,
        bl.CommissionAmount
    FROM SalesCommissionBatchLine bl
    INNER JOIN SalesCommissionBatch b ON b.Id = bl.BatchId
    INNER JOIN UserMaster u ON u.Id = b.UserId
    WHERE b.Status <> 'Rejected'
      AND (@FromDate IS NULL OR b.ToDate >= @FromDate)
      AND (@ToDate IS NULL OR b.FromDate <= @ToDate)
      AND (@UserId IS NULL OR b.UserId = @UserId)
      AND (@BatchId IS NULL OR b.Id = @BatchId)
    ORDER BY u.FullName, b.FromDate, bl.PaymentDate;
END
GO

-- ── History Report ─────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'sp_SalesCommissionHistoryReport') AND type = 'P')
BEGIN
    EXEC('CREATE PROCEDURE sp_SalesCommissionHistoryReport AS SELECT 1')
END
GO

ALTER PROCEDURE sp_SalesCommissionHistoryReport
    @FromDate DATE         = NULL,
    @ToDate   DATE         = NULL,
    @UserId   INT          = NULL,
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
        b.CommissionTypeSnapshot AS CommissionType,
        b.TotalPayments,
        b.TotalSalesAmount,
        b.GrossCommission,
        b.DeductionAmount,
        b.NetCommission,
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
    FROM SalesCommissionBatch b
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

-- 068: Create Payout Commission / Hourly Pay tables
-- =====================================================

-- 1) PayoutConfiguration — one row per user, Either Hourly OR Commission
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayoutConfiguration')
BEGIN
    CREATE TABLE [dbo].[PayoutConfiguration] (
        [Id]                      INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]                  INT             NOT NULL,
        [PayoutModel]             NVARCHAR(20)    NOT NULL,  -- 'Hourly' or 'Commission'
        [HourlyRate]              DECIMAL(18,2)   NULL,
        [DefaultCommissionAmount] DECIMAL(18,2)   NULL,
        [EffectiveFrom]           DATE            NOT NULL,
        [IsActive]                BIT             NOT NULL DEFAULT 1,
        [CreatedById]             INT             NOT NULL,
        [CreatedAt]               DATETIME2       NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]               DATETIME2       NULL,
        CONSTRAINT FK_PayoutConfiguration_User      FOREIGN KEY ([UserId])      REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_PayoutConfiguration_CreatedBy FOREIGN KEY ([CreatedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT CK_PayoutConfiguration_Model     CHECK ([PayoutModel] IN ('Hourly','Commission')),
        CONSTRAINT UQ_PayoutConfiguration_UserId    UNIQUE ([UserId])
    );

    CREATE NONCLUSTERED INDEX IX_PayoutConfiguration_UserId
        ON [dbo].[PayoutConfiguration] ([UserId]) WHERE [IsActive] = 1;
END
GO

-- 2) PayoutCommissionRule — hierarchical rate rules per user
--    Priority: ProjectModuleId=30 > TaskCategoryId=20 > TaskTypeId=10 > Default=0
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayoutCommissionRule')
BEGIN
    CREATE TABLE [dbo].[PayoutCommissionRule] (
        [Id]              INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]          INT             NOT NULL,
        [TaskTypeId]      INT             NULL,
        [TaskCategoryId]  INT             NULL,
        [ProjectModuleId] INT             NULL,
        [Amount]          DECIMAL(18,2)   NOT NULL,
        [Priority]        INT             NOT NULL DEFAULT 0,  -- auto-computed: 30/20/10/0
        [EffectiveFrom]   DATE            NOT NULL,
        [IsActive]        BIT             NOT NULL DEFAULT 1,
        [CreatedById]     INT             NOT NULL,
        [CreatedAt]       DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_PayoutCommissionRule_User      FOREIGN KEY ([UserId])          REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_PayoutCommissionRule_TaskType  FOREIGN KEY ([TaskTypeId])      REFERENCES [TaskTypeMaster]([Id]),
        CONSTRAINT FK_PayoutCommissionRule_TaskCat   FOREIGN KEY ([TaskCategoryId])  REFERENCES [TaskCategoryMaster]([Id]),
        CONSTRAINT FK_PayoutCommissionRule_Project   FOREIGN KEY ([ProjectModuleId]) REFERENCES [ProjectModuleMaster]([Id]),
        CONSTRAINT FK_PayoutCommissionRule_CreatedBy FOREIGN KEY ([CreatedById])     REFERENCES [UserMaster]([Id])
    );

    CREATE NONCLUSTERED INDEX IX_PayoutCommissionRule_UserId
        ON [dbo].[PayoutCommissionRule] ([UserId], [IsActive]) INCLUDE ([Priority], [Amount]);
END
GO

-- 3) PayoutBatch — one per user per generated period
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayoutBatch')
BEGIN
    CREATE TABLE [dbo].[PayoutBatch] (
        [Id]                     INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId]                 INT             NOT NULL,
        [FromDate]               DATE            NOT NULL,
        [ToDate]                 DATE            NOT NULL,
        [PayoutModel]            NVARCHAR(20)    NOT NULL,    -- snapshot at generation
        [HourlyRateSnapshot]     DECIMAL(18,2)   NULL,
        [TotalMinutes]           INT             NOT NULL DEFAULT 0,
        [TotalTasks]             INT             NOT NULL DEFAULT 0,
        [GrossAmount]            DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [DeductionAmount]        DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [NetAmount]              DECIMAL(18,2)   NOT NULL DEFAULT 0,
        [Status]                 NVARCHAR(30)    NOT NULL DEFAULT 'Draft',
        -- Draft / PendingApproval / L1Approved / Approved / Paid / Rejected
        [Remarks]                NVARCHAR(500)   NULL,
        [GeneratedAt]            DATETIME2       NOT NULL DEFAULT GETDATE(),
        [GeneratedById]          INT             NOT NULL,
        -- Settlement fields (populated when Status = Paid)
        [SettlementAmount]       DECIMAL(18,2)   NULL,
        [SettlementDate]         DATE            NULL,
        [SettledAt]              DATETIME2       NULL,
        [SettledById]            INT             NULL,
        [SettlementMode]         NVARCHAR(50)    NULL,
        [SettlementReferenceNo]  NVARCHAR(100)   NULL,
        [SettlementBankId]       INT             NULL,
        [SettlementRemarks]      NVARCHAR(500)   NULL,
        CONSTRAINT FK_PayoutBatch_User        FOREIGN KEY ([UserId])         REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_PayoutBatch_GeneratedBy FOREIGN KEY ([GeneratedById])  REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_PayoutBatch_SettledBy   FOREIGN KEY ([SettledById])    REFERENCES [UserMaster]([Id]),
        CONSTRAINT FK_PayoutBatch_Bank        FOREIGN KEY ([SettlementBankId]) REFERENCES [BankMaster]([Id]),
        CONSTRAINT CK_PayoutBatch_Status      CHECK ([Status] IN ('Draft','PendingApproval','L1Approved','Approved','Paid','Rejected'))
    );

    CREATE NONCLUSTERED INDEX IX_PayoutBatch_UserId_Status
        ON [dbo].[PayoutBatch] ([UserId], [Status]) INCLUDE ([FromDate], [ToDate], [NetAmount]);
    CREATE NONCLUSTERED INDEX IX_PayoutBatch_Status
        ON [dbo].[PayoutBatch] ([Status]) INCLUDE ([UserId], [FromDate], [ToDate]);
END
GO

-- 4) PayoutBatchLine — one per task in the batch
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayoutBatchLine')
BEGIN
    CREATE TABLE [dbo].[PayoutBatchLine] (
        [Id]                INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BatchId]           INT             NOT NULL,
        [TaskId]            INT             NOT NULL,
        [TaskTitle]         NVARCHAR(300)   NOT NULL,        -- snapshot
        [TaskTypeName]      NVARCHAR(100)   NOT NULL,        -- snapshot
        [TaskCategoryName]  NVARCHAR(100)   NOT NULL,        -- snapshot
        [ProjectModuleName] NVARCHAR(200)   NULL,            -- snapshot
        [TimeSpentMinutes]  INT             NOT NULL DEFAULT 0,
        [RateApplied]       DECIMAL(18,2)   NOT NULL,
        [RateSource]        NVARCHAR(30)    NOT NULL,        -- 'Hourly','Project','Category','TaskType','Default'
        [Amount]            DECIMAL(18,2)   NOT NULL,
        [TaskCompletedAt]   DATETIME2       NULL,
        CONSTRAINT FK_PayoutBatchLine_Batch FOREIGN KEY ([BatchId]) REFERENCES [PayoutBatch]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_PayoutBatchLine_Task  FOREIGN KEY ([TaskId])  REFERENCES [DailyTaskLog]([Id]),
        CONSTRAINT CK_PayoutBatchLine_RateSource CHECK ([RateSource] IN ('Hourly','Project','Category','TaskType','Default'))
    );

    CREATE NONCLUSTERED INDEX IX_PayoutBatchLine_BatchId
        ON [dbo].[PayoutBatchLine] ([BatchId]);
    CREATE UNIQUE NONCLUSTERED INDEX UX_PayoutBatchLine_TaskId
        ON [dbo].[PayoutBatchLine] ([TaskId]);
        -- Ensures a task can only appear in one batch
END
GO

-- 5) PayoutApprovalHistory — approval/rejection trail per batch
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PayoutApprovalHistory')
BEGIN
    CREATE TABLE [dbo].[PayoutApprovalHistory] (
        [Id]            INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [BatchId]       INT             NOT NULL,
        [ApproverLevel] INT             NOT NULL,  -- 1 = Core Member, 2 = Admin/Finance
        [ApprovedById]  INT             NOT NULL,
        [Status]        NVARCHAR(20)    NOT NULL,  -- 'Approved' or 'Rejected'
        [Remarks]       NVARCHAR(500)   NULL,
        [ApprovedAt]    DATETIME2       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_PayoutApprovalHistory_Batch    FOREIGN KEY ([BatchId])      REFERENCES [PayoutBatch]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_PayoutApprovalHistory_Approver FOREIGN KEY ([ApprovedById]) REFERENCES [UserMaster]([Id]),
        CONSTRAINT CK_PayoutApprovalHistory_Status   CHECK ([Status] IN ('Approved','Rejected'))
    );

    CREATE NONCLUSTERED INDEX IX_PayoutApprovalHistory_BatchId
        ON [dbo].[PayoutApprovalHistory] ([BatchId]);
END
GO

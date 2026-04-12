-- ============================================================
-- 062 – Daily Task Log Module Tables
-- ============================================================

-- 1) Task Type Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTypeMaster')
BEGIN
    CREATE TABLE TaskTypeMaster (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100)  NOT NULL,
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM TaskTypeMaster)
BEGIN
    INSERT INTO TaskTypeMaster (Name) VALUES ('Development'), ('Support');
END

-- 2) Task Category Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskCategoryMaster')
BEGIN
    CREATE TABLE TaskCategoryMaster (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(100)  NOT NULL,
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM TaskCategoryMaster)
BEGIN
    INSERT INTO TaskCategoryMaster (Name) VALUES
        ('Bug Fix'), ('New Feature'), ('Enhancement'), ('Client Issue'), ('Internal Task');
END

-- 3) Project Module Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProjectModuleMaster')
BEGIN
    CREATE TABLE ProjectModuleMaster (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(200)  NOT NULL,
        Description NVARCHAR(500)  NULL,
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedAt   DATETIME       NOT NULL DEFAULT GETDATE()
    );
END

-- 4) Daily Task Log (core table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DailyTaskLog')
BEGIN
    CREATE TABLE DailyTaskLog (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        UserId              INT             NOT NULL,
        AssignedToUserId    INT             NULL,
        TaskDate            DATE            NOT NULL,
        TaskTypeId          INT             NOT NULL,
        TaskCategoryId      INT             NOT NULL,
        TaskTitle           NVARCHAR(300)   NOT NULL,
        Description         NVARCHAR(MAX)   NULL,
        TicketId            INT             NULL,
        ProjectModuleId     INT             NULL,
        TimeSpentMinutes    INT             NOT NULL DEFAULT 0,
        Status              NVARCHAR(50)    NOT NULL DEFAULT 'Pending',
        CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
        UpdatedAt           DATETIME        NULL,

        CONSTRAINT FK_DailyTaskLog_User          FOREIGN KEY (UserId)          REFERENCES UserMaster(Id),
        CONSTRAINT FK_DailyTaskLog_AssignedTo    FOREIGN KEY (AssignedToUserId) REFERENCES UserMaster(Id),
        CONSTRAINT FK_DailyTaskLog_TaskType      FOREIGN KEY (TaskTypeId)      REFERENCES TaskTypeMaster(Id),
        CONSTRAINT FK_DailyTaskLog_TaskCategory  FOREIGN KEY (TaskCategoryId)  REFERENCES TaskCategoryMaster(Id),
        CONSTRAINT FK_DailyTaskLog_Ticket        FOREIGN KEY (TicketId)        REFERENCES HelpDeskTicket(Id),
        CONSTRAINT FK_DailyTaskLog_Project       FOREIGN KEY (ProjectModuleId) REFERENCES ProjectModuleMaster(Id)
    );
END

-- Add AssignedToUserId column if missing (for existing databases)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DailyTaskLog') AND name = 'AssignedToUserId')
BEGIN
    ALTER TABLE DailyTaskLog ADD AssignedToUserId INT NULL;
    ALTER TABLE DailyTaskLog ADD CONSTRAINT FK_DailyTaskLog_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES UserMaster(Id);
END

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DailyTaskLog_UserId_TaskDate')
    CREATE INDEX IX_DailyTaskLog_UserId_TaskDate ON DailyTaskLog(UserId, TaskDate DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DailyTaskLog_TaskDate')
    CREATE INDEX IX_DailyTaskLog_TaskDate ON DailyTaskLog(TaskDate DESC);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DailyTaskLog_TicketId')
    CREATE INDEX IX_DailyTaskLog_TicketId ON DailyTaskLog(TicketId) WHERE TicketId IS NOT NULL;

-- 5) Task Approval Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DailyTaskApproval')
BEGIN
    CREATE TABLE DailyTaskApproval (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        TaskId      INT             NOT NULL,
        ApprovedBy  INT             NOT NULL,
        Status      NVARCHAR(50)    NOT NULL DEFAULT 'Pending',   -- Pending / Approved / Rejected
        Remarks     NVARCHAR(500)   NULL,
        ApprovedAt  DATETIME        NULL,
        CreatedAt   DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_DailyTaskApproval_Task FOREIGN KEY (TaskId)     REFERENCES DailyTaskLog(Id),
        CONSTRAINT FK_DailyTaskApproval_User FOREIGN KEY (ApprovedBy) REFERENCES UserMaster(Id)
    );
END

-- 6) Task Time Log (child records for effort tracking)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTimeLog')
BEGIN
    CREATE TABLE TaskTimeLog (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        TaskId              INT             NOT NULL,
        UserId              INT             NOT NULL,
        LogDate             DATE            NOT NULL,
        TimeSpentMinutes    INT             NOT NULL DEFAULT 0,
        Remarks             NVARCHAR(500)   NULL,
        CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TaskTimeLog_Task FOREIGN KEY (TaskId) REFERENCES DailyTaskLog(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskTimeLog_User FOREIGN KEY (UserId) REFERENCES UserMaster(Id)
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskTimeLog_TaskId')
    CREATE INDEX IX_TaskTimeLog_TaskId ON TaskTimeLog(TaskId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskTimeLog_LogDate')
    CREATE INDEX IX_TaskTimeLog_LogDate ON TaskTimeLog(LogDate DESC);

-- ============================================
-- Help Desk Ticketing System Tables
-- ============================================

-- 1. Ticket Category Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketCategoryMaster')
BEGIN
    CREATE TABLE [dbo].[TicketCategoryMaster] (
        [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CategoryName] NVARCHAR(100)  NOT NULL,
        [Description]  NVARCHAR(300)  NULL,
        [IsActive]     BIT            NOT NULL DEFAULT 1,
        [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_TicketCategoryMaster_CategoryName'
      AND object_id = OBJECT_ID('TicketCategoryMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_TicketCategoryMaster_CategoryName
        ON dbo.TicketCategoryMaster(CategoryName);
END

-- Seed default categories
IF NOT EXISTS (SELECT 1 FROM TicketCategoryMaster WHERE CategoryName = 'General Inquiry')
    INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive) VALUES ('General Inquiry', 'General questions and inquiries', 1);
IF NOT EXISTS (SELECT 1 FROM TicketCategoryMaster WHERE CategoryName = 'Technical Issue')
    INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive) VALUES ('Technical Issue', 'Software bugs or technical problems', 1);
IF NOT EXISTS (SELECT 1 FROM TicketCategoryMaster WHERE CategoryName = 'Billing')
    INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive) VALUES ('Billing', 'Billing and payment related issues', 1);
IF NOT EXISTS (SELECT 1 FROM TicketCategoryMaster WHERE CategoryName = 'Feature Request')
    INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive) VALUES ('Feature Request', 'New feature or enhancement requests', 1);
IF NOT EXISTS (SELECT 1 FROM TicketCategoryMaster WHERE CategoryName = 'Account Issue')
    INSERT INTO TicketCategoryMaster (CategoryName, Description, IsActive) VALUES ('Account Issue', 'Account access and profile issues', 1);

-- 2. Ticket Priority Master
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketPriorityMaster')
BEGIN
    CREATE TABLE [dbo].[TicketPriorityMaster] (
        [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PriorityName] NVARCHAR(50)   NOT NULL,
        [ColorCode]    NVARCHAR(20)   NULL,
        [SortOrder]    INT            NOT NULL DEFAULT 0,
        [SlaResponseHours]   INT      NULL,
        [SlaResolutionHours] INT      NULL,
        [IsActive]     BIT            NOT NULL DEFAULT 1,
        [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_TicketPriorityMaster_PriorityName'
      AND object_id = OBJECT_ID('TicketPriorityMaster'))
BEGIN
    CREATE UNIQUE INDEX UX_TicketPriorityMaster_PriorityName
        ON dbo.TicketPriorityMaster(PriorityName);
END

-- Seed default priorities
IF NOT EXISTS (SELECT 1 FROM TicketPriorityMaster WHERE PriorityName = 'Low')
    INSERT INTO TicketPriorityMaster (PriorityName, ColorCode, SortOrder, SlaResponseHours, SlaResolutionHours) VALUES ('Low', '#6b7280', 1, 24, 72);
IF NOT EXISTS (SELECT 1 FROM TicketPriorityMaster WHERE PriorityName = 'Medium')
    INSERT INTO TicketPriorityMaster (PriorityName, ColorCode, SortOrder, SlaResponseHours, SlaResolutionHours) VALUES ('Medium', '#f59e0b', 2, 8, 48);
IF NOT EXISTS (SELECT 1 FROM TicketPriorityMaster WHERE PriorityName = 'High')
    INSERT INTO TicketPriorityMaster (PriorityName, ColorCode, SortOrder, SlaResponseHours, SlaResolutionHours) VALUES ('High', '#ef4444', 3, 4, 24);
IF NOT EXISTS (SELECT 1 FROM TicketPriorityMaster WHERE PriorityName = 'Critical')
    INSERT INTO TicketPriorityMaster (PriorityName, ColorCode, SortOrder, SlaResponseHours, SlaResolutionHours) VALUES ('Critical', '#dc2626', 4, 1, 8);

-- 3. Help Desk Ticket (main ticket table)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='HelpDeskTicket')
BEGIN
    CREATE TABLE [dbo].[HelpDeskTicket] (
        [Id]              INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TicketNumber]    NVARCHAR(20)   NOT NULL,
        [Subject]         NVARCHAR(300)  NOT NULL,
        [Description]     NVARCHAR(MAX)  NOT NULL,
        [CategoryId]      INT            NOT NULL,
        [PriorityId]      INT            NOT NULL,
        [Status]          NVARCHAR(30)   NOT NULL DEFAULT 'Open',
        [CreatedById]     INT            NOT NULL,
        [AssignedToId]    INT            NULL,
        [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE(),
        [UpdatedAt]       DATETIME       NULL,
        [FirstResponseAt] DATETIME       NULL,
        [ResolvedAt]      DATETIME       NULL,
        [ClosedAt]        DATETIME       NULL,
        CONSTRAINT FK_HelpDeskTicket_Category FOREIGN KEY (CategoryId) REFERENCES TicketCategoryMaster(Id),
        CONSTRAINT FK_HelpDeskTicket_Priority FOREIGN KEY (PriorityId) REFERENCES TicketPriorityMaster(Id),
        CONSTRAINT FK_HelpDeskTicket_CreatedBy FOREIGN KEY (CreatedById) REFERENCES UserMaster(Id),
        CONSTRAINT FK_HelpDeskTicket_AssignedTo FOREIGN KEY (AssignedToId) REFERENCES UserMaster(Id)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_HelpDeskTicket_TicketNumber'
      AND object_id = OBJECT_ID('HelpDeskTicket'))
BEGIN
    CREATE UNIQUE INDEX UX_HelpDeskTicket_TicketNumber
        ON dbo.HelpDeskTicket(TicketNumber);
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_HelpDeskTicket_Status'
      AND object_id = OBJECT_ID('HelpDeskTicket'))
BEGIN
    CREATE INDEX IX_HelpDeskTicket_Status ON dbo.HelpDeskTicket(Status);
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_HelpDeskTicket_AssignedToId'
      AND object_id = OBJECT_ID('HelpDeskTicket'))
BEGIN
    CREATE INDEX IX_HelpDeskTicket_AssignedToId ON dbo.HelpDeskTicket(AssignedToId);
END

-- 4. Ticket Message (threaded conversation)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketMessage')
BEGIN
    CREATE TABLE [dbo].[TicketMessage] (
        [Id]          INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TicketId]    INT            NOT NULL,
        [SenderId]    INT            NOT NULL,
        [Message]     NVARCHAR(MAX)  NOT NULL,
        [IsInternal]  BIT            NOT NULL DEFAULT 0,
        [CreatedAt]   DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketMessage_Ticket FOREIGN KEY (TicketId) REFERENCES HelpDeskTicket(Id),
        CONSTRAINT FK_TicketMessage_Sender FOREIGN KEY (SenderId) REFERENCES UserMaster(Id)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_TicketMessage_TicketId'
      AND object_id = OBJECT_ID('TicketMessage'))
BEGIN
    CREATE INDEX IX_TicketMessage_TicketId ON dbo.TicketMessage(TicketId);
END

-- 5. Ticket Attachment
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketAttachment')
BEGIN
    CREATE TABLE [dbo].[TicketAttachment] (
        [Id]          INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TicketId]    INT            NOT NULL,
        [MessageId]   INT            NULL,
        [FileName]    NVARCHAR(300)  NOT NULL,
        [FilePath]    NVARCHAR(500)  NOT NULL,
        [FileSize]    BIGINT         NULL,
        [UploadedById] INT           NOT NULL,
        [CreatedAt]   DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketAttachment_Ticket FOREIGN KEY (TicketId) REFERENCES HelpDeskTicket(Id),
        CONSTRAINT FK_TicketAttachment_Message FOREIGN KEY (MessageId) REFERENCES TicketMessage(Id),
        CONSTRAINT FK_TicketAttachment_UploadedBy FOREIGN KEY (UploadedById) REFERENCES UserMaster(Id)
    );
END

-- 6. Ticket Audit Log (status changes, assignments, etc.)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='TicketAuditLog')
BEGIN
    CREATE TABLE [dbo].[TicketAuditLog] (
        [Id]          INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TicketId]    INT            NOT NULL,
        [Action]      NVARCHAR(100)  NOT NULL,
        [OldValue]    NVARCHAR(200)  NULL,
        [NewValue]    NVARCHAR(200)  NULL,
        [PerformedById] INT          NOT NULL,
        [CreatedAt]   DATETIME       NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketAuditLog_Ticket FOREIGN KEY (TicketId) REFERENCES HelpDeskTicket(Id),
        CONSTRAINT FK_TicketAuditLog_PerformedBy FOREIGN KEY (PerformedById) REFERENCES UserMaster(Id)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_TicketAuditLog_TicketId'
      AND object_id = OBJECT_ID('TicketAuditLog'))
BEGIN
    CREATE INDEX IX_TicketAuditLog_TicketId ON dbo.TicketAuditLog(TicketId);
END

-- Seed Ticket roles into RoleMaster
IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName = 'Ticket Agent')
    INSERT INTO RoleMaster (RoleName, IsActive) VALUES ('Ticket Agent', 1);
IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName = 'Ticket Admin')
    INSERT INTO RoleMaster (RoleName, IsActive) VALUES ('Ticket Admin', 1);
IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName = 'ClientTicket')
    INSERT INTO RoleMaster (RoleName, IsActive) VALUES ('ClientTicket', 1);

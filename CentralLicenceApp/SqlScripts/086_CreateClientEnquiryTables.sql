-- ============================================================
-- 086 – Client Enquiry module: tables, default lists, menu and permissions
-- Idempotent: safe to run more than once.
-- ============================================================

-- ── Masters ──
IF OBJECT_ID('EnquirySource') IS NULL
    CREATE TABLE EnquirySource (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE, SortOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1);
GO
IF OBJECT_ID('EnquiryBusinessType') IS NULL
    CREATE TABLE EnquiryBusinessType (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE, SortOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1);
GO
IF OBJECT_ID('EnquiryLostReason') IS NULL
    CREATE TABLE EnquiryLostReason (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE, SortOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1);
GO
IF OBJECT_ID('EnquiryCallOutcome') IS NULL
    CREATE TABLE EnquiryCallOutcome (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE, SortOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1,
        IsConnected BIT NOT NULL DEFAULT 0, RetryHours INT NULL);
GO
IF OBJECT_ID('EnquiryStage') IS NULL
    CREATE TABLE EnquiryStage (Id INT IDENTITY PRIMARY KEY, Name NVARCHAR(100) NOT NULL UNIQUE, SortOrder INT NOT NULL DEFAULT 0, IsActive BIT NOT NULL DEFAULT 1,
        StageCode NVARCHAR(30) NULL,            -- fixed code used by automation; NULL for stages added by users
        DefaultFollowUpDays INT NOT NULL DEFAULT 2,
        IsClosed BIT NOT NULL DEFAULT 0,
        ColorCode NVARCHAR(10) NOT NULL DEFAULT N'#6366f1');
GO

IF NOT EXISTS (SELECT 1 FROM EnquirySource)
    INSERT INTO EnquirySource (Name, SortOrder) VALUES (N'Phone Call',1),(N'Website',2),(N'Referral',3),(N'Existing Client',4),(N'Exhibition / Event',5),(N'Social Media',6),(N'Walk-in',7),(N'Other',8);
IF NOT EXISTS (SELECT 1 FROM EnquiryBusinessType)
    INSERT INTO EnquiryBusinessType (Name, SortOrder) VALUES (N'Hospital',1),(N'Clinic',2),(N'Diagnostic Lab',3),(N'Pharmacy',4),(N'Other',5);
IF NOT EXISTS (SELECT 1 FROM EnquiryLostReason)
    INSERT INTO EnquiryLostReason (Name, SortOrder) VALUES (N'Price',1),(N'Competitor',2),(N'No Budget',3),(N'No Response',4),(N'Requirement Mismatch',5),(N'Other',6);
IF NOT EXISTS (SELECT 1 FROM EnquiryCallOutcome)
    INSERT INTO EnquiryCallOutcome (Name, SortOrder, IsConnected, RetryHours) VALUES
        (N'Connected',1,1,NULL),(N'No Answer',2,0,2),(N'Busy',3,0,2),(N'Call Back Later',4,1,24),(N'Wrong Number',5,0,NULL),(N'Not Interested',6,1,NULL);
IF NOT EXISTS (SELECT 1 FROM EnquiryStage)
    INSERT INTO EnquiryStage (Name, SortOrder, StageCode, DefaultFollowUpDays, IsClosed, ColorCode) VALUES
        (N'New',1,N'NEW',1,0,N'#64748b'),(N'Contacted',2,N'CONTACTED',2,0,N'#0ea5e9'),(N'Qualified',3,N'QUALIFIED',3,0,N'#6366f1'),
        (N'Demo Scheduled',4,N'DEMO_SCHEDULED',1,0,N'#8b5cf6'),(N'Demo Done',5,N'DEMO_DONE',2,0,N'#a855f7'),(N'Quotation Sent',6,N'QUOTATION_SENT',3,0,N'#f59e0b'),
        (N'Negotiation',7,N'NEGOTIATION',2,0,N'#f97316'),(N'Won',8,N'WON',0,1,N'#16a34a'),(N'Lost',9,N'LOST',0,1,N'#dc2626');
GO

-- ── Enquiry Client Master (separate from Party Master) ──
IF OBJECT_ID('EnquiryClient') IS NULL
    CREATE TABLE EnquiryClient (
        Id INT IDENTITY PRIMARY KEY,
        ClientCode NVARCHAR(20) NULL,
        CompanyName NVARCHAR(200) NOT NULL,
        BusinessTypeId INT NULL REFERENCES EnquiryBusinessType(Id),
        GSTIN NVARCHAR(20) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Website NVARCHAR(200) NULL,
        ExistingSoftware NVARCHAR(200) NULL,
        SizeInfo NVARCHAR(100) NULL,
        ReferredBy NVARCHAR(200) NULL,
        Notes NVARCHAR(1000) NULL,
        LinkedClientCode NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedByUserId INT NOT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL);
GO
IF OBJECT_ID('EnquiryClientContact') IS NULL
BEGIN
    CREATE TABLE EnquiryClientContact (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryClientId INT NOT NULL REFERENCES EnquiryClient(Id) ON DELETE CASCADE,
        Name NVARCHAR(150) NOT NULL,
        Role NVARCHAR(100) NULL,
        Mobile NVARCHAR(20) NULL,
        WhatsApp NVARCHAR(20) NULL,
        Email NVARCHAR(200) NULL,
        IsPrimary BIT NOT NULL DEFAULT 0,
        WhatsAppOptOut BIT NOT NULL DEFAULT 0);
    CREATE INDEX IX_EnquiryClientContact_Mobile ON EnquiryClientContact(Mobile);
    CREATE INDEX IX_EnquiryClientContact_Email ON EnquiryClientContact(Email);
END
GO

-- ── Enquiry ──
IF OBJECT_ID('Enquiry') IS NULL
BEGIN
    CREATE TABLE Enquiry (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryNo NVARCHAR(30) NULL,
        EnquiryClientId INT NOT NULL REFERENCES EnquiryClient(Id),
        Title NVARCHAR(200) NOT NULL,
        Requirement NVARCHAR(2000) NULL,
        SourceId INT NULL REFERENCES EnquirySource(Id),
        StageId INT NOT NULL REFERENCES EnquiryStage(Id),
        PriorityId INT NULL REFERENCES TicketPriorityMaster(Id),
        OwnerUserId INT NOT NULL REFERENCES UserMaster(Id),
        ExpectedValue DECIMAL(18,2) NULL,
        ExpectedCloseDate DATE NULL,
        NextFollowUpAt DATETIME NULL,
        LostReasonId INT NULL REFERENCES EnquiryLostReason(Id),
        LostRemarks NVARCHAR(500) NULL,
        QuotationId INT NULL,                   -- link to an existing quotation
        IsOnHold BIT NOT NULL DEFAULT 0,
        ResumeDate DATE NULL,
        ClosedAt DATETIME NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedByUserId INT NOT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME NULL);
    CREATE INDEX IX_Enquiry_Owner_Next ON Enquiry(OwnerUserId, NextFollowUpAt);
    CREATE INDEX IX_Enquiry_Client ON Enquiry(EnquiryClientId);
END
GO
IF OBJECT_ID('EnquiryProduct') IS NULL
    CREATE TABLE EnquiryProduct (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        ProductId INT NOT NULL REFERENCES ProductMaster(Id),
        CONSTRAINT UQ_EnquiryProduct UNIQUE (EnquiryId, ProductId));
GO
IF OBJECT_ID('EnquiryCollaborator') IS NULL
    CREATE TABLE EnquiryCollaborator (
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        UserId INT NOT NULL REFERENCES UserMaster(Id),
        CONSTRAINT PK_EnquiryCollaborator PRIMARY KEY (EnquiryId, UserId));
GO
IF OBJECT_ID('EnquiryFollowUp') IS NULL
BEGIN
    CREATE TABLE EnquiryFollowUp (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        FollowUpType NVARCHAR(20) NOT NULL DEFAULT N'Call',   -- Call, WhatsApp, Email, Visit
        DueAt DATETIME NOT NULL,
        AssignedToUserId INT NOT NULL REFERENCES UserMaster(Id),
        Purpose NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'Open',         -- Open, Done, Cancelled
        Result NVARCHAR(1000) NULL,
        CompletedAt DATETIME NULL,
        RescheduleCount INT NOT NULL DEFAULT 0,
        LastRescheduleReason NVARCHAR(300) NULL,
        CreatedByUserId INT NOT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE());
    CREATE INDEX IX_EnquiryFollowUp_User ON EnquiryFollowUp(AssignedToUserId, Status, DueAt);
END
GO
IF OBJECT_ID('EnquiryCallLog') IS NULL
BEGIN
    CREATE TABLE EnquiryCallLog (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        ContactId INT NULL REFERENCES EnquiryClientContact(Id),
        FollowUpId INT NULL,
        Direction NVARCHAR(10) NOT NULL DEFAULT N'Outgoing',
        PhoneNumber NVARCHAR(20) NULL,
        StartedAt DATETIME NOT NULL,
        DurationSeconds INT NOT NULL DEFAULT 0,
        OutcomeId INT NOT NULL REFERENCES EnquiryCallOutcome(Id),
        Notes NVARCHAR(1000) NULL,
        CalledByUserId INT NOT NULL REFERENCES UserMaster(Id));
    CREATE INDEX IX_EnquiryCallLog_Enquiry ON EnquiryCallLog(EnquiryId, StartedAt DESC);
END
GO
IF OBJECT_ID('EnquiryDemo') IS NULL
BEGIN
    CREATE TABLE EnquiryDemo (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        ProductId INT NOT NULL REFERENCES ProductMaster(Id),
        Mode NVARCHAR(10) NOT NULL DEFAULT N'Online',          -- Online, OnSite
        ScheduledAt DATETIME NOT NULL,
        DurationMinutes INT NOT NULL DEFAULT 60,
        PresenterUserId INT NOT NULL REFERENCES UserMaster(Id),
        MeetingLink NVARCHAR(500) NULL,                        -- link, or address for on-site demos
        Attendees NVARCHAR(500) NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT N'Scheduled',     -- Scheduled, Rescheduled, Completed, NoShow, Cancelled
        Rating INT NULL,
        InterestLevel NVARCHAR(10) NULL,                       -- High, Medium, Low
        Feedback NVARCHAR(1000) NULL,
        CreatedByUserId INT NOT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE());
    CREATE INDEX IX_EnquiryDemo_Schedule ON EnquiryDemo(ScheduledAt, PresenterUserId);
END
GO
IF OBJECT_ID('EnquiryDemoAttendee') IS NULL
    CREATE TABLE EnquiryDemoAttendee (
        DemoId INT NOT NULL REFERENCES EnquiryDemo(Id) ON DELETE CASCADE,
        ContactId INT NOT NULL REFERENCES EnquiryClientContact(Id),
        CONSTRAINT PK_EnquiryDemoAttendee PRIMARY KEY (DemoId, ContactId));
GO
IF OBJECT_ID('EnquiryActivity') IS NULL
BEGIN
    CREATE TABLE EnquiryActivity (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        ActivityType NVARCHAR(20) NOT NULL,                    -- Created, Stage, FollowUp, Call, Demo, Note, Quotation, Reassign, Attachment, Edit
        Summary NVARCHAR(1500) NOT NULL,
        UserId INT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE());
    CREATE INDEX IX_EnquiryActivity_Enquiry ON EnquiryActivity(EnquiryId, CreatedAt DESC);
END
GO
IF OBJECT_ID('EnquiryAttachment') IS NULL
    CREATE TABLE EnquiryAttachment (
        Id INT IDENTITY PRIMARY KEY,
        EnquiryId INT NOT NULL REFERENCES Enquiry(Id) ON DELETE CASCADE,
        FileName NVARCHAR(260) NOT NULL,
        StoredName NVARCHAR(100) NOT NULL,
        UploadedByUserId INT NOT NULL REFERENCES UserMaster(Id),
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE());
GO
IF OBJECT_ID('EnquiryReminderLog') IS NULL
    CREATE TABLE EnquiryReminderLog (
        Id INT IDENTITY PRIMARY KEY,
        RefType NVARCHAR(20) NOT NULL,      -- FollowUp, Demo, Digest
        RefId INT NOT NULL,
        ReminderType NVARCHAR(30) NOT NULL,
        UserId INT NOT NULL,
        SentAt DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT UQ_EnquiryReminderLog UNIQUE (RefType, RefId, ReminderType, UserId));
GO

-- ── Menu: CRM › Client Enquiry › pages ──
IF OBJECT_ID('MenuMaster') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Client Enquiry')
   AND EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)
BEGIN
    DECLARE @Crm INT = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL);
    DECLARE @Sort INT = ISNULL((SELECT MAX(SortOrder) FROM MenuMaster WHERE ParentId = @Crm), 0) + 1;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@Crm, N'Client Enquiry', N'Collapsible', NULL, NULL, N'bi bi-telephone-inbound', @Sort, 1);
    DECLARE @Parent INT = SCOPE_IDENTITY();

    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive) VALUES
        (@Parent, N'Enquiry Dashboard', N'Link', N'ClientEnquiry',   N'Dashboard', N'bi bi-speedometer2',  1, 1),
        (@Parent, N'Call List',         N'Link', N'ClientEnquiry',   N'CallList',  N'bi bi-telephone',     2, 1),
        (@Parent, N'Enquiries',         N'Link', N'ClientEnquiry',   N'Index',     N'bi bi-kanban',        3, 1),
        (@Parent, N'Enquiry Clients',   N'Link', N'EnquiryClient',   N'Index',     N'bi bi-building',      4, 1),
        (@Parent, N'Demo Calendar',     N'Link', N'EnquiryDemo',     N'Index',     N'bi bi-calendar-event',5, 1),
        (@Parent, N'Enquiry Reports',   N'Link', N'ClientEnquiry',   N'Reports',   N'bi bi-bar-chart-line',6, 1),
        (@Parent, N'Enquiry Masters',   N'Link', N'EnquiryMaster',   N'Index',     N'bi bi-collection',    7, 1);

    -- Which permissions each menu supports
    INSERT INTO MenuPermissionMap (MenuId, PermissionId)
        SELECT @Parent, Id FROM PermissionMaster WHERE PermissionKey = N'View';
    INSERT INTO MenuPermissionMap (MenuId, PermissionId)
        SELECT m.Id, p.Id FROM MenuMaster m CROSS JOIN PermissionMaster p
        WHERE m.ParentId = @Parent AND p.PermissionKey IN (N'View', N'Create', N'Edit', N'Delete', N'Export');

    -- Full access: Administrator, Ticket Admin
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT r.Id, mp.MenuId, mp.PermissionId FROM RoleMaster r
        CROSS JOIN MenuPermissionMap mp
        INNER JOIN MenuMaster m ON m.Id = mp.MenuId
        WHERE r.RoleName IN (N'Administrator', N'Ticket Admin') AND (m.Id = @Parent OR m.ParentId = @Parent);

    -- CRM parent must be viewable by those roles
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT r.Id, @Crm, p.Id FROM RoleMaster r CROSS JOIN PermissionMaster p
        WHERE r.RoleName IN (N'Administrator', N'Ticket Admin') AND p.PermissionKey = N'View'
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap x WHERE x.RoleId = r.Id AND x.MenuId = @Crm AND x.PermissionId = p.Id);
END
GO

-- Access is limited to Administrator and Ticket Admin: remove any other grants on these menus.
IF OBJECT_ID('MenuMaster') IS NOT NULL
BEGIN
    DELETE g FROM RolePermissionMap g
    INNER JOIN RoleMaster r ON r.Id = g.RoleId
    INNER JOIN MenuMaster m ON m.Id = g.MenuId
    LEFT JOIN MenuMaster p ON p.Id = m.ParentId
    WHERE (m.MenuName = N'Client Enquiry' OR p.MenuName = N'Client Enquiry')
      AND r.RoleName NOT IN (N'Administrator', N'Ticket Admin');
END
GO

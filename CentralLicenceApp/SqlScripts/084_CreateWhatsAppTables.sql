-- ============================================================
-- 084 – WhatsApp (WaSender) configuration, message log and menu
-- Idempotent: safe to run more than once.
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppConfiguration')
BEGIN
    CREATE TABLE [dbo].[WhatsAppConfiguration] (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        ConfigName          NVARCHAR(100)  NOT NULL DEFAULT N'Primary WaSender API',
        ApiUrl              NVARCHAR(500)  NOT NULL DEFAULT N'https://wasenderapi.com/api/send-message',
        AuthHeaderKey       NVARCHAR(50)   NOT NULL DEFAULT N'Authorization',
        ApiKeyEncrypted     NVARCHAR(MAX)  NULL,          -- Data Protection ciphertext, never sent to the browser
        ApiKeyLast4         NVARCHAR(4)    NULL,
        DefaultCountryCode  NVARCHAR(10)   NOT NULL DEFAULT N'+91',
        SenderPhone         NVARCHAR(50)   NULL,
        IsEnabled           BIT            NOT NULL DEFAULT 1,
        MinGapMs            INT            NOT NULL DEFAULT 5500,
        LastTestedDate      DATETIME       NULL,
        LastTestResult      NVARCHAR(500)  NULL,
        CreatedBy           NVARCHAR(100)  NULL,
        CreatedDate         DATETIME       NOT NULL DEFAULT GETDATE(),
        ModifiedBy          NVARCHAR(100)  NULL,
        ModifiedDate        DATETIME       NULL
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WhatsAppLog')
BEGIN
    CREATE TABLE [dbo].[WhatsAppLog] (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        ConfigId        INT            NULL,
        ModuleCode      NVARCHAR(20)   NOT NULL,
        ModuleRefId     INT            NULL,
        RecipientPhone  NVARCHAR(50)   NOT NULL,
        MessageText     NVARCHAR(MAX)  NULL,
        IsSuccess       BIT            NOT NULL DEFAULT 0,
        MsgId           NVARCHAR(100)  NULL,
        Jid             NVARCHAR(100)  NULL,
        Status          NVARCHAR(50)   NULL,
        ApiResponse     NVARCHAR(MAX)  NULL,
        ErrorMessage    NVARCHAR(1000) NULL,
        SentBy          NVARCHAR(100)  NULL,
        SentDate        DATETIME       NOT NULL DEFAULT GETDATE()
    );
    CREATE INDEX IX_WhatsAppLog_SentDate ON [dbo].[WhatsAppLog](SentDate DESC);
    CREATE INDEX IX_WhatsAppLog_Module ON [dbo].[WhatsAppLog](ModuleCode, ModuleRefId);
END
GO

-- Mirror every email notification to the recipient user's WhatsApp (UserMaster.PhoneNumber)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WhatsAppConfiguration') AND name = 'NotifyOnEmail')
    ALTER TABLE [dbo].[WhatsAppConfiguration] ADD NotifyOnEmail BIT NOT NULL DEFAULT 1;
GO

-- Menu: Administration › WhatsApp Config (View, Edit) for Administrator
IF OBJECT_ID('MenuMaster') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM MenuMaster WHERE ControllerName = N'WhatsAppConfig')
BEGIN
    DECLARE @Admin INT = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL);
    IF @Admin IS NOT NULL
    BEGIN
        DECLARE @Sort INT = ISNULL((SELECT SortOrder FROM MenuMaster WHERE ParentId = @Admin AND ControllerName = N'EmailLog'), 6);
        INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
            VALUES (@Admin, N'WhatsApp Config', N'Link', N'WhatsAppConfig', N'Index', N'bi bi-whatsapp', @Sort, 1);
        DECLARE @M INT = SCOPE_IDENTITY();

        INSERT INTO MenuPermissionMap (MenuId, PermissionId)
            SELECT @M, Id FROM PermissionMaster WHERE PermissionKey IN (N'View', N'Edit');

        DECLARE @R INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Administrator');
        IF @R IS NOT NULL
            INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
                SELECT @R, @M, Id FROM PermissionMaster WHERE PermissionKey IN (N'View', N'Edit');
    END
END
GO

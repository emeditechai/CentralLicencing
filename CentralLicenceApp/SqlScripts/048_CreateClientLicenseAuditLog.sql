-- Migration 048: Create ClientLicenseAuditLog table
-- Tracks changes to ExpiryDate and AMC_Expireddate on ClientAppLicense records

IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'ClientLicenseAuditLog'
)
BEGIN
    CREATE TABLE dbo.ClientLicenseAuditLog (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        ClientLicenseId INT            NOT NULL,
        ClientCode      NVARCHAR(100)  NOT NULL DEFAULT '',
        ClientName      NVARCHAR(500)  NOT NULL DEFAULT '',
        ProductType     NVARCHAR(100)  NOT NULL DEFAULT '',
        FieldChanged    NVARCHAR(100)  NOT NULL,   -- 'ExpiryDate' or 'AMCExpiryDate'
        OldValue        NVARCHAR(50)   NULL,        -- formatted date string
        NewValue        NVARCHAR(50)   NULL,
        ChangedBy       NVARCHAR(256)  NOT NULL DEFAULT '',
        ChangedAt       DATETIME       NOT NULL DEFAULT GETDATE()
    );

    CREATE INDEX IX_ClientLicenseAuditLog_ClientLicenseId
        ON dbo.ClientLicenseAuditLog (ClientLicenseId);

    CREATE INDEX IX_ClientLicenseAuditLog_ChangedAt
        ON dbo.ClientLicenseAuditLog (ChangedAt DESC);

    PRINT 'ClientLicenseAuditLog table created.';
END
ELSE
BEGIN
    PRINT 'ClientLicenseAuditLog table already exists — skipped.';
END

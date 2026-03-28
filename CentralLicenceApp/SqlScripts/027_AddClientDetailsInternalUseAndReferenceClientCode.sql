IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ClientDetails')
      AND name = 'IsInternalUse'
)
BEGIN
    ALTER TABLE dbo.ClientDetails
    ADD IsInternalUse BIT NOT NULL CONSTRAINT DF_ClientDetails_IsInternalUse DEFAULT (0);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ClientDetails')
      AND name = 'ReferenceClientCode'
)
BEGIN
    ALTER TABLE dbo.ClientDetails
    ADD ReferenceClientCode VARCHAR(20) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ClientDetails_ReferenceClientCode'
      AND object_id = OBJECT_ID('dbo.ClientDetails')
)
BEGIN
    CREATE INDEX IX_ClientDetails_ReferenceClientCode ON dbo.ClientDetails(ReferenceClientCode);
END;
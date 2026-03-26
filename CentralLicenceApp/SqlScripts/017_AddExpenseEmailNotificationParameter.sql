IF NOT EXISTS (
    SELECT *
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CompanySettings')
      AND name = 'IsExpenseEmailNotificationRequired'
)
BEGIN
    ALTER TABLE dbo.CompanySettings
    ADD IsExpenseEmailNotificationRequired BIT NOT NULL
        CONSTRAINT DF_CompanySettings_IsExpenseEmailNotificationRequired DEFAULT 0;
END;
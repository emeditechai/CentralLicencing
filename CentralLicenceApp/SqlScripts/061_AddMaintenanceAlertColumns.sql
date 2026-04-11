-- ============================================================
-- Add Maintenance Alert columns to ClientAppLicense
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ClientAppLicense') AND name = 'IsDisplayAlerts')
BEGIN
    ALTER TABLE ClientAppLicense ADD
        IsDisplayAlerts  BIT            NOT NULL DEFAULT 0,
        AlertStartDate   DATE           NULL,
        AlertStartTime   TIME           NULL,
        AlertEndDate     DATE           NULL,
        AlertEndTime     TIME           NULL,
        AlertMessage     NVARCHAR(1000) NULL;
END
GO

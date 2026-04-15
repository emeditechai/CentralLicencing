-- 071: Add IsSalesAgent flag to UserMaster
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'IsSalesAgent'
)
BEGIN
    ALTER TABLE UserMaster ADD IsSalesAgent BIT NOT NULL DEFAULT 0;
END
GO

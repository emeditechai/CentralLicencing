-- 035: Add DigitalSignaturePath column to UserMaster
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'DigitalSignaturePath'
)
BEGIN
    ALTER TABLE UserMaster
    ADD DigitalSignaturePath NVARCHAR(500) NULL;
END

-- =============================================
-- Migration 005: Add ProfileImagePath to UserMaster
-- Run on: Central_Lic_DB
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'ProfileImagePath'
)
BEGIN
    ALTER TABLE UserMaster ADD ProfileImagePath NVARCHAR(300) NULL;
    PRINT 'Added ProfileImagePath column';
END
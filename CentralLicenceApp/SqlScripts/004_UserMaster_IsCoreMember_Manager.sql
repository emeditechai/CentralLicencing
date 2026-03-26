-- =============================================
-- Migration 004: Add IsCoreMember and ManagerId
-- Run on: Central_Lic_DB
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'IsCoreMember'
)
BEGIN
    ALTER TABLE UserMaster ADD IsCoreMember BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsCoreMember column';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'ManagerId'
)
BEGIN
    ALTER TABLE UserMaster ADD ManagerId INT NULL;
    PRINT 'Added ManagerId column';
END

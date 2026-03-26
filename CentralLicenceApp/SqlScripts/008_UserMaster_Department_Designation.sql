-- =============================================
-- Migration 008: Add DepartmentId and DesignationId to UserMaster
-- Run on: Central_Lic_DB
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'DepartmentId'
)
BEGIN
    ALTER TABLE UserMaster ADD DepartmentId INT NULL;
    PRINT 'Added DepartmentId column';
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'DesignationId'
)
BEGIN
    ALTER TABLE UserMaster ADD DesignationId INT NULL;
    PRINT 'Added DesignationId column';
END
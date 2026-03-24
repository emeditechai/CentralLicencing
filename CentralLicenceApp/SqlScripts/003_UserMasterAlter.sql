-- ============================================================
-- Script: 003_UserMasterAlter.sql
-- Description: Create LocationMaster table and add new columns
--              (PhoneNumber, LocationId, IsEmployee, EmployeeCode)
--              to UserMaster.
-- Database: Central_Lic_DB
-- Run once against your database.
-- ============================================================

USE [Central_Lic_DB];
GO

-- 1. LocationMaster table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LocationMaster')
BEGIN
    CREATE TABLE [dbo].[LocationMaster] (
        [Id]        INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name]      NVARCHAR(100) NOT NULL,
        [IsActive]  BIT           NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table LocationMaster created.';

    INSERT INTO [dbo].[LocationMaster] ([Name]) VALUES
        ('Kolkata'), ('Mumbai'), ('Delhi'), ('Bengaluru'),
        ('Chennai'), ('Hyderabad'), ('Pune'), ('Ahmedabad');
    PRINT 'LocationMaster seeded with default cities.';
END
ELSE
    PRINT 'Table LocationMaster already exists.';
GO

-- 2. PhoneNumber column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [PhoneNumber] NVARCHAR(20) NULL;
    PRINT 'Column PhoneNumber added to UserMaster.';
END
GO

-- 3. LocationId column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'LocationId')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [LocationId] INT NULL
        REFERENCES [dbo].[LocationMaster]([Id]);
    PRINT 'Column LocationId added to UserMaster.';
END
GO

-- 4. IsEmployee column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'IsEmployee')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [IsEmployee] BIT NOT NULL DEFAULT 0;
    PRINT 'Column IsEmployee added to UserMaster.';
END
GO

-- 5. EmployeeCode column
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'EmployeeCode')
BEGIN
    ALTER TABLE [dbo].[UserMaster] ADD [EmployeeCode] NVARCHAR(50) NULL;
    PRINT 'Column EmployeeCode added to UserMaster.';
END
GO

-- 6. Filtered unique index on EmployeeCode (NULLs are excluded from uniqueness)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UQ_UserMaster_EmployeeCode'
      AND object_id = OBJECT_ID('UserMaster'))
BEGIN
    CREATE UNIQUE INDEX [UQ_UserMaster_EmployeeCode]
        ON [dbo].[UserMaster] ([EmployeeCode])
        WHERE [EmployeeCode] IS NOT NULL;
    PRINT 'Unique index on EmployeeCode created.';
END
GO

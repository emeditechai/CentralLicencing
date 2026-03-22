-- ============================================================
-- Script: 001_CreateRoleMaster.sql
-- Description: Create RoleMaster table and seed roles
-- Database: Central_Lic_DB
-- ============================================================

USE [Central_Lic_DB];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RoleMaster')
BEGIN
    CREATE TABLE [dbo].[RoleMaster] (
        [Id]          INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RoleName]    NVARCHAR(50)  NOT NULL,
        [Description] NVARCHAR(200) NULL,
        [IsActive]    BIT           NOT NULL DEFAULT 1,
        [CreatedAt]   DATETIME      NOT NULL DEFAULT GETDATE()
    );
    PRINT 'Table RoleMaster created successfully.';
END
ELSE
    PRINT 'Table RoleMaster already exists.';
GO

-- Seed default roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[RoleMaster] WHERE [RoleName] = 'Administrator')
BEGIN
    INSERT INTO [dbo].[RoleMaster] ([RoleName], [Description], [IsActive])
    VALUES ('Administrator', 'Full access – manage all licences, users, and settings', 1);
    PRINT 'Role Administrator inserted.';
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[RoleMaster] WHERE [RoleName] = 'Staff')
BEGIN
    INSERT INTO [dbo].[RoleMaster] ([RoleName], [Description], [IsActive])
    VALUES ('Staff', 'Read-only access – view licences and validation history', 1);
    PRINT 'Role Staff inserted.';
END
GO

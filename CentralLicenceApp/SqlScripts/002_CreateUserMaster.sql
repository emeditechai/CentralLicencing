-- ============================================================
-- Script: 002_CreateUserMaster.sql
-- Description: Create UserMaster table and seed admin user
-- Database: Central_Lic_DB
-- Password for seed user "admin" = BCrypt hash of "Admin@1234"
-- ============================================================

USE [Central_Lic_DB];
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserMaster')
BEGIN
    CREATE TABLE [dbo].[UserMaster] (
        [Id]            INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Username]      NVARCHAR(100)  NOT NULL UNIQUE,
        [Email]         NVARCHAR(200)  NOT NULL UNIQUE,
        [PasswordHash]  NVARCHAR(500)  NOT NULL,
        [FullName]      NVARCHAR(200)  NULL,
        [PhoneNumber]   NVARCHAR(20)   NULL,
        [DateOfBirth]   DATE           NULL,
        [DateOfJoining] DATE           NULL,
        [RoleId]        INT            NOT NULL REFERENCES [dbo].[RoleMaster]([Id]),
        [IsActive]      BIT            NOT NULL DEFAULT 1,
        [CreatedAt]     DATETIME       NOT NULL DEFAULT GETDATE(),
        [LastLoginDate] DATETIME       NULL
    );
    PRINT 'Table UserMaster created successfully.';
END
ELSE
    PRINT 'Table UserMaster already exists.';
GO

-- Seed default Administrator (password: Admin@1234)
-- BCrypt hash generated at work time; regenerate via app if needed.
DECLARE @AdminRoleId INT = (SELECT TOP 1 Id FROM [dbo].[RoleMaster] WHERE RoleName = 'Administrator');
DECLARE @StaffRoleId INT = (SELECT TOP 1 Id FROM [dbo].[RoleMaster] WHERE RoleName = 'Staff');

IF NOT EXISTS (SELECT 1 FROM [dbo].[UserMaster] WHERE [Username] = 'admin')
BEGIN
    -- BCrypt hash of "Admin@1234"
    INSERT INTO [dbo].[UserMaster] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [IsActive])
    VALUES (
        'admin',
        'admin@centrallicence.com',
        '$2a$11$KE4BsNV5Q0vFJSb8V8qZR.RBOlBKVlMQf6D3oQdpWzXFRBDZJYVrS',
        'System Administrator',
        @AdminRoleId,
        1
    );
    PRINT 'Default admin user created. Username: admin | Password: Admin@1234';
END

-- Seed a default Staff user (password: Staff@1234)
IF NOT EXISTS (SELECT 1 FROM [dbo].[UserMaster] WHERE [Username] = 'staff')
BEGIN
    INSERT INTO [dbo].[UserMaster] ([Username], [Email], [PasswordHash], [FullName], [RoleId], [IsActive])
    VALUES (
        'staff',
        'staff@centrallicence.com',
        '$2a$11$X9Xr8LiPJNL0.QEGXd2iMeS1CRSCo4nVlb4Q8.qDsw8I2sDd0BL5C',
        'Staff Member',
        @StaffRoleId,
        1
    );
    PRINT 'Default staff user created. Username: staff | Password: Staff@1234';
END
GO

-- =========================================================================
-- 077_CreateDynamicAuthorizationTables.sql
-- Dynamic authorization & DB-driven menu system
-- Tables: MenuMaster, PermissionMaster, MenuPermissionMap,
--         RolePermissionMap, UserPermissionMap
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MenuMaster')
BEGIN
    CREATE TABLE [dbo].[MenuMaster] (
        [Id]             INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ParentId]       INT NULL,
        [MenuName]       NVARCHAR(100) NOT NULL,
        [MenuType]       NVARCHAR(20) NOT NULL DEFAULT 'Link', -- Section | Collapsible | Link
        [ControllerName] NVARCHAR(100) NULL,
        [ActionName]     NVARCHAR(100) NULL,
        [IconClass]      NVARCHAR(80) NULL,
        [SortOrder]      INT NOT NULL DEFAULT 0,
        [IsActive]       BIT NOT NULL DEFAULT 1,
        [CreatedAt]      DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_MenuMaster_Parent FOREIGN KEY (ParentId)
            REFERENCES [dbo].[MenuMaster](Id)
    );
    CREATE INDEX IX_MenuMaster_Parent ON [dbo].[MenuMaster](ParentId);
    CREATE INDEX IX_MenuMaster_CtrlAct ON [dbo].[MenuMaster](ControllerName, ActionName);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PermissionMaster')
BEGIN
    CREATE TABLE [dbo].[PermissionMaster] (
        [Id]            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [PermissionKey] NVARCHAR(40) NOT NULL UNIQUE,
        [DisplayName]   NVARCHAR(80) NOT NULL,
        [SortOrder]     INT NOT NULL DEFAULT 0,
        [IsActive]      BIT NOT NULL DEFAULT 1,
        [CreatedAt]     DATETIME NOT NULL DEFAULT GETDATE()
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MenuPermissionMap')
BEGIN
    CREATE TABLE [dbo].[MenuPermissionMap] (
        [MenuId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        CONSTRAINT PK_MenuPermissionMap PRIMARY KEY (MenuId, PermissionId),
        CONSTRAINT FK_MenuPermissionMap_Menu FOREIGN KEY (MenuId)
            REFERENCES [dbo].[MenuMaster](Id) ON DELETE CASCADE,
        CONSTRAINT FK_MenuPermissionMap_Perm FOREIGN KEY (PermissionId)
            REFERENCES [dbo].[PermissionMaster](Id) ON DELETE CASCADE
    );
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissionMap')
BEGIN
    CREATE TABLE [dbo].[RolePermissionMap] (
        [RoleId]       INT NOT NULL,
        [MenuId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT PK_RolePermissionMap PRIMARY KEY (RoleId, MenuId, PermissionId),
        CONSTRAINT FK_RolePermMap_Role FOREIGN KEY (RoleId)
            REFERENCES [dbo].[RoleMaster](Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermMap_Menu FOREIGN KEY (MenuId)
            REFERENCES [dbo].[MenuMaster](Id),
        CONSTRAINT FK_RolePermMap_Perm FOREIGN KEY (PermissionId)
            REFERENCES [dbo].[PermissionMaster](Id)
    );
    CREATE INDEX IX_RolePermMap_Role ON [dbo].[RolePermissionMap](RoleId);
END

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPermissionMap')
BEGIN
    CREATE TABLE [dbo].[UserPermissionMap] (
        [UserId]       INT NOT NULL,
        [MenuId]       INT NOT NULL,
        [PermissionId] INT NOT NULL,
        [IsGranted]    BIT NOT NULL DEFAULT 1,   -- 1=Allow override, 0=Deny override
        [CreatedAt]    DATETIME NOT NULL DEFAULT GETDATE(),
        [CreatedBy]    INT NULL,
        CONSTRAINT PK_UserPermissionMap PRIMARY KEY (UserId, MenuId, PermissionId),
        CONSTRAINT FK_UserPermMap_User FOREIGN KEY (UserId)
            REFERENCES [dbo].[UserMaster](Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserPermMap_Menu FOREIGN KEY (MenuId)
            REFERENCES [dbo].[MenuMaster](Id),
        CONSTRAINT FK_UserPermMap_Perm FOREIGN KEY (PermissionId)
            REFERENCES [dbo].[PermissionMaster](Id)
    );
    CREATE INDEX IX_UserPermMap_User ON [dbo].[UserPermissionMap](UserId);
END

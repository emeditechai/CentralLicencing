-- =========================================================================
-- 080_SeedRolePermissionMap.sql
-- Seeds RolePermissionMap based on the static sidebar's original role rules.
-- Idempotent: only inserts when RolePermissionMap is empty.
-- Requires 077, 078, 079 to be applied first.
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM RolePermissionMap)
BEGIN
    SET NOCOUNT ON;

    DECLARE @R_Administrator INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Administrator');
    DECLARE @R_Staff INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Staff');
    DECLARE @R_Finance INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Finance');
    DECLARE @R_Ticket_Admin INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Ticket Admin');
    DECLARE @R_Ticket_Agent INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'Ticket Agent');
    DECLARE @R_ClientTicket INT = (SELECT Id FROM RoleMaster WHERE RoleName = N'ClientTicket');

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Main' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Licences' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Validation History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Audit Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Company Settings' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Company Settings' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Company Settings' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Company Settings' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Department' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Designation' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Financial Year' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Engine' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Engine' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Engine' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Engine' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Templates' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Logs' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Logs' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Logs' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Email Logs' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'App File Upload' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Menu Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Role Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'User Permissions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Security' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Administration' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Requests' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reimburse')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reimburse'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reimburse')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reimburse'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reimbursement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expenses & Advance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotations' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Process' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Quotation & Invoices' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Category' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'eProduct Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Product Rate List' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Party Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Bank Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payment Modes' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Terms & Conditions' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Configuration' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Payout' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Config' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Invoice Assignment' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Commission Batches' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Approval Inbox' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Desk' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Commission' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Business Unit' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Details' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Expense Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Settlement Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Daily Collection Register' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Client Due Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Summary Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Detail Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'History Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_ClientTicket, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_ClientTicket IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_ClientTicket AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Staff, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Staff IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Staff AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Finance, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Finance IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Finance AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_ClientTicket, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_ClientTicket IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_ClientTicket AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_ClientTicket, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_ClientTicket IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_ClientTicket AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_ClientTicket, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_ClientTicket IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_ClientTicket AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Tickets' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Management' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'My Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Team Task Log' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Sub Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Ticket Priorities' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Types' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Task Categories' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project / Module' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Master' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL)) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Analytics Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Analytics Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Analytics Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Analytics Dashboard' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Agent Performance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'SLA Compliance' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Timesheet Report' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Employee Productivity' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Project Effort' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Payout History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Summary' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm Detail' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Administrator, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Administrator IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Administrator AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Admin, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Admin IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Admin AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View'));
    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT @R_Ticket_Agent, (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))), (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export')
        WHERE @R_Ticket_Agent IS NOT NULL
          AND NOT EXISTS (SELECT 1 FROM RolePermissionMap WHERE RoleId = @R_Ticket_Agent AND MenuId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM Sales Comm History' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Reports' AND ParentId = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'CRM' AND ParentId IS NULL))) AND PermissionId = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export'));

END
GO

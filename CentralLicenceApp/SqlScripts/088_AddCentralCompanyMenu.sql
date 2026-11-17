-- ============================================================
-- 088 – Licences › Central Company menu (Administrator only, view)
-- Reads the existing Central_Company and Central_Users tables. Idempotent.
-- ============================================================

IF OBJECT_ID('MenuMaster') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM MenuMaster WHERE ControllerName = N'CentralCompany')
   AND EXISTS (SELECT 1 FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL)
BEGIN
    DECLARE @Lic INT = (SELECT TOP 1 Id FROM MenuMaster WHERE MenuName = N'Licences' AND ParentId IS NULL);
    DECLARE @Sort INT = ISNULL((SELECT MAX(SortOrder) FROM MenuMaster WHERE ParentId = @Lic), 0) + 1;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@Lic, N'Central Company', N'Link', N'CentralCompany', N'Index', N'bi bi-buildings', @Sort, 1);
    DECLARE @M INT = SCOPE_IDENTITY();

    INSERT INTO MenuPermissionMap (MenuId, PermissionId)
        SELECT @M, Id FROM PermissionMaster WHERE PermissionKey = N'View';

    INSERT INTO RolePermissionMap (RoleId, MenuId, PermissionId)
        SELECT r.Id, @M, p.Id FROM RoleMaster r CROSS JOIN PermissionMaster p
        WHERE r.RoleName = N'Administrator' AND p.PermissionKey = N'View';
END
GO

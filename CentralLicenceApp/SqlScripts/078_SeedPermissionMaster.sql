-- =========================================================================
-- 078_SeedPermissionMaster.sql
-- Seeds the standard permission keys used by the dynamic authorization layer.
-- Idempotent: existing rows are not duplicated.
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'View')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'View', N'View', 10, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Create')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Create', N'Create', 20, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Edit')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Edit', N'Edit', 30, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Delete')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Delete', N'Delete', 40, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Approve')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Approve', N'Approve', 50, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Reject')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Reject', N'Reject', 60, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Export')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Export', N'Export', 70, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Print')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Print', N'Print', 80, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Cancel')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Cancel', N'Cancel', 90, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Refund')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Refund', N'Refund', 100, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Settle')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Settle', N'Settle', 110, 1);
IF NOT EXISTS (SELECT 1 FROM PermissionMaster WHERE PermissionKey = N'Reimburse')
    INSERT INTO PermissionMaster (PermissionKey, DisplayName, SortOrder, IsActive) VALUES (N'Reimburse', N'Reimburse', 120, 1);
GO

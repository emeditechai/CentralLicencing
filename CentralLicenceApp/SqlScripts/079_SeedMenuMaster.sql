-- =========================================================================
-- 079_SeedMenuMaster.sql
-- Seeds MenuMaster + MenuPermissionMap to mirror the application sidebar.
-- Idempotent: only inserts when MenuMaster is empty.
-- =========================================================================

IF NOT EXISTS (SELECT 1 FROM MenuMaster)
BEGIN
    SET NOCOUNT ON;

    DECLARE @P_View INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'View');
    DECLARE @P_Create INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Create');
    DECLARE @P_Edit INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Edit');
    DECLARE @P_Delete INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Delete');
    DECLARE @P_Approve INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Approve');
    DECLARE @P_Reject INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reject');
    DECLARE @P_Export INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Export');
    DECLARE @P_Print INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Print');
    DECLARE @P_Cancel INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Cancel');
    DECLARE @P_Refund INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Refund');
    DECLARE @P_Settle INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Settle');
    DECLARE @P_Reimburse INT = (SELECT Id FROM PermissionMaster WHERE PermissionKey = N'Reimburse');

    DECLARE @M1 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'Main', N'Section', NULL, NULL, NULL, 0, 1);
    SET @M1 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M1, @P_View);

    DECLARE @M2 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M1, N'Dashboard', N'Link', N'Dashboard', N'Index', N'bi bi-speedometer2', 0, 1);
    SET @M2 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M2, @P_View);

    DECLARE @M3 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'Licences', N'Collapsible', NULL, NULL, N'bi bi-card-checklist', 1, 1);
    SET @M3 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M3, @P_View);

    DECLARE @M4 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M3, N'Client Licences', N'Link', N'ClientLicense', N'Index', N'bi bi-card-checklist', 0, 1);
    SET @M4 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M4, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M4, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M4, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M4, @P_Delete);

    DECLARE @M5 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M3, N'Validation History', N'Link', N'LicenseHistory', N'Index', N'bi bi-clock-history', 1, 1);
    SET @M5 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M5, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M5, @P_Export);

    DECLARE @M6 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M3, N'Audit Log', N'Link', N'LicenseAuditLog', N'Index', N'bi bi-clipboard2-pulse', 2, 1);
    SET @M6 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M6, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M6, @P_Export);

    DECLARE @M7 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'Administration', N'Collapsible', NULL, NULL, N'bi bi-sliders2-vertical', 2, 1);
    SET @M7 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M7, @P_View);

    DECLARE @M8 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'User Management', N'Link', N'User', N'Index', N'bi bi-people', 0, 1);
    SET @M8 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M8, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M8, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M8, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M8, @P_Delete);

    DECLARE @M9 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Role Management', N'Link', N'Role', N'Index', N'bi bi-diagram-3', 1, 1);
    SET @M9 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M9, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M9, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M9, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M9, @P_Delete);

    DECLARE @M10 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Company Settings', N'Link', N'CompanySettings', N'Index', N'bi bi-buildings', 2, 1);
    SET @M10 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M10, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M10, @P_Edit);

    DECLARE @M11 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Master', N'Collapsible', NULL, NULL, N'bi bi-collection', 3, 1);
    SET @M11 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M11, @P_View);

    DECLARE @M12 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M11, N'Employee Department', N'Link', N'EmployeeDepartment', N'Index', N'bi bi-building', 0, 1);
    SET @M12 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M12, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M12, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M12, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M12, @P_Delete);

    DECLARE @M13 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M11, N'Employee Designation', N'Link', N'EmployeeDesignation', N'Index', N'bi bi-person-workspace', 1, 1);
    SET @M13 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M13, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M13, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M13, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M13, @P_Delete);

    DECLARE @M14 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M11, N'Financial Year', N'Link', N'FinancialYearMaster', N'Index', N'bi bi-calendar-range', 2, 1);
    SET @M14 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M14, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M14, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M14, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M14, @P_Delete);

    DECLARE @M15 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Email Engine', N'Link', N'EmailConfig', N'Index', N'bi bi-send-check', 4, 1);
    SET @M15 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M15, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M15, @P_Edit);

    DECLARE @M16 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Email Templates', N'Link', N'EmailTemplate', N'Index', N'bi bi-file-earmark-text', 5, 1);
    SET @M16 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M16, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M16, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M16, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M16, @P_Delete);

    DECLARE @M17 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Email Logs', N'Link', N'EmailLog', N'Index', N'bi bi-journal-text', 6, 1);
    SET @M17 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M17, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M17, @P_Export);

    DECLARE @M18 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'App File Upload', N'Link', N'AppUpload', N'Index', N'bi bi-cloud-upload', 7, 1);
    SET @M18 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M18, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M18, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M18, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M18, @P_Delete);

    DECLARE @M19 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M7, N'Security', N'Collapsible', NULL, NULL, N'bi bi-shield-lock', 8, 1);
    SET @M19 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M19, @P_View);

    DECLARE @M20 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M19, N'Menu Management', N'Link', N'MenuManagement', N'Index', N'bi bi-list-nested', 0, 1);
    SET @M20 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M20, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M20, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M20, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M20, @P_Delete);

    DECLARE @M21 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M19, N'Role Permissions', N'Link', N'RolePermission', N'Index', N'bi bi-diagram-2', 1, 1);
    SET @M21 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M21, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M21, @P_Edit);

    DECLARE @M22 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M19, N'User Permissions', N'Link', N'UserPermission', N'Index', N'bi bi-person-check', 2, 1);
    SET @M22 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M22, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M22, @P_Edit);

    DECLARE @M23 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'Business Unit', N'Collapsible', NULL, NULL, N'bi bi-diagram-2', 3, 1);
    SET @M23 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M23, @P_View);

    DECLARE @M24 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M23, N'Expenses & Advance', N'Collapsible', NULL, NULL, N'bi bi-arrow-left-right', 0, 1);
    SET @M24 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M24, @P_View);

    DECLARE @M25 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M24, N'Expense Requests', N'Link', N'ExpenseRequest', N'Index', N'bi bi-wallet2', 0, 1);
    SET @M25 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M25, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M25, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M25, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M25, @P_Delete);

    DECLARE @M26 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M24, N'Approval Inbox', N'Link', N'ExpenseRequest', N'Approvals', N'bi bi-check2-square', 1, 1);
    SET @M26 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M26, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M26, @P_Approve);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M26, @P_Reject);

    DECLARE @M27 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M24, N'Reimbursement Desk', N'Link', N'ExpenseRequest', N'FinanceDesk', N'bi bi-cash-stack', 2, 1);
    SET @M27 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M27, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M27, @P_Reimburse);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M27, @P_Settle);

    DECLARE @M28 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M23, N'Quotation & Invoices', N'Collapsible', NULL, NULL, N'bi bi-file-earmark-text', 1, 1);
    SET @M28 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M28, @P_View);

    DECLARE @M29 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M28, N'Quotations', N'Link', N'Quotation', N'Index', N'bi bi-file-earmark-check', 0, 1);
    SET @M29 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M29, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M29, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M29, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M29, @P_Cancel);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M29, @P_Print);

    DECLARE @M30 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M28, N'Invoices', N'Link', N'Invoice', N'Index', N'bi bi-receipt-cutoff', 1, 1);
    SET @M30 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_Cancel);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_Print);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M30, @P_Refund);

    DECLARE @M31 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M28, N'Payment Process', N'Link', N'InvoicePayment', N'Index', N'bi bi-cash-coin', 2, 1);
    SET @M31 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M31, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M31, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M31, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M31, @P_Refund);

    DECLARE @M32 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M23, N'Master', N'Collapsible', NULL, NULL, N'bi bi-collection', 2, 1);
    SET @M32 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M32, @P_View);

    DECLARE @M33 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Expense Category', N'Link', N'ExpenseCategory', N'Index', N'bi bi-receipt', 0, 1);
    SET @M33 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M33, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M33, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M33, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M33, @P_Delete);

    DECLARE @M34 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'eProduct Master', N'Link', N'ProductMaster', N'Index', N'bi bi-box-seam', 1, 1);
    SET @M34 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M34, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M34, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M34, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M34, @P_Delete);

    DECLARE @M35 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Product Rate List', N'Link', N'ProductRate', N'Index', N'bi bi-tags', 2, 1);
    SET @M35 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M35, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M35, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M35, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M35, @P_Delete);

    DECLARE @M36 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Party Master', N'Link', N'PartyMaster', N'Index', N'bi bi-people', 3, 1);
    SET @M36 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M36, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M36, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M36, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M36, @P_Delete);

    DECLARE @M37 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Bank Master', N'Link', N'BankMaster', N'Index', N'bi bi-bank2', 4, 1);
    SET @M37 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M37, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M37, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M37, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M37, @P_Delete);

    DECLARE @M38 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Payment Modes', N'Link', N'PaymentMode', N'Index', N'bi bi-credit-card-2-front', 5, 1);
    SET @M38 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M38, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M38, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M38, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M38, @P_Delete);

    DECLARE @M39 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M32, N'Terms & Conditions', N'Link', N'TermsConditionTemplate', N'Index', N'bi bi-file-text', 6, 1);
    SET @M39 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M39, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M39, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M39, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M39, @P_Delete);

    DECLARE @M40 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M23, N'Task Payout', N'Collapsible', NULL, NULL, N'bi bi-gear', 3, 1);
    SET @M40 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M40, @P_View);

    DECLARE @M41 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M40, N'Payout Configuration', N'Link', N'PayoutConfiguration', N'Index', N'bi bi-sliders', 0, 1);
    SET @M41 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M41, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M41, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M41, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M41, @P_Delete);

    DECLARE @M42 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M40, N'Payout Batches', N'Link', N'PayoutBatch', N'Index', N'bi bi-collection', 1, 1);
    SET @M42 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M42, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M42, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M42, @P_Edit);

    DECLARE @M43 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M40, N'Approval Inbox', N'Link', N'PayoutBatch', N'ApprovalInbox', N'bi bi-inbox', 2, 1);
    SET @M43 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M43, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M43, @P_Approve);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M43, @P_Reject);

    DECLARE @M44 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M40, N'Settlement Desk', N'Link', N'PayoutBatch', N'SettlementDesk', N'bi bi-cash-stack', 3, 1);
    SET @M44 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M44, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M44, @P_Settle);

    DECLARE @M45 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M23, N'Sales Commission', N'Collapsible', NULL, NULL, N'bi bi-percent', 4, 1);
    SET @M45 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M45, @P_View);

    DECLARE @M46 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M45, N'Commission Config', N'Link', N'SalesCommissionConfig', N'Index', N'bi bi-sliders', 0, 1);
    SET @M46 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M46, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M46, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M46, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M46, @P_Delete);

    DECLARE @M47 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M45, N'Invoice Assignment', N'Link', N'SalesInvoiceAssignment', N'Index', N'bi bi-link-45deg', 1, 1);
    SET @M47 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M47, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M47, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M47, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M47, @P_Delete);

    DECLARE @M48 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M45, N'Commission Batches', N'Link', N'SalesCommissionBatch', N'Index', N'bi bi-collection', 2, 1);
    SET @M48 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M48, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M48, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M48, @P_Edit);

    DECLARE @M49 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M45, N'Approval Inbox', N'Link', N'SalesCommissionBatch', N'ApprovalInbox', N'bi bi-inbox', 3, 1);
    SET @M49 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M49, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M49, @P_Approve);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M49, @P_Reject);

    DECLARE @M50 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M45, N'Settlement Desk', N'Link', N'SalesCommissionBatch', N'SettlementDesk', N'bi bi-cash-stack', 4, 1);
    SET @M50 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M50, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M50, @P_Settle);

    DECLARE @M51 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'Reports', N'Collapsible', NULL, NULL, N'bi bi-bar-chart-line', 4, 1);
    SET @M51 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M51, @P_View);

    DECLARE @M52 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Client Details', N'Link', N'Reports', N'ClientDetails', N'bi bi-file-earmark-bar-graph', 0, 1);
    SET @M52 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M52, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M52, @P_Export);

    DECLARE @M53 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Expense Report', N'Link', N'Reports', N'ExpenseReport', N'bi bi-wallet2', 1, 1);
    SET @M53 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M53, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M53, @P_Export);

    DECLARE @M54 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Settlement Report', N'Link', N'Reports', N'SettlementReport', N'bi bi-cash-stack', 2, 1);
    SET @M54 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M54, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M54, @P_Export);

    DECLARE @M55 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Daily Collection Register', N'Link', N'Reports', N'DailyCollectionRegister', N'bi bi-cash-coin', 3, 1);
    SET @M55 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M55, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M55, @P_Export);

    DECLARE @M56 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Client Due Report', N'Link', N'Reports', N'ClientDueReport', N'bi bi-exclamation-triangle', 4, 1);
    SET @M56 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M56, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M56, @P_Export);

    DECLARE @M57 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Summary Report', N'Link', N'PayoutReport', N'Summary', N'bi bi-bar-chart', 5, 1);
    SET @M57 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M57, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M57, @P_Export);

    DECLARE @M58 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Detail Report', N'Link', N'PayoutReport', N'Detail', N'bi bi-list-columns-reverse', 6, 1);
    SET @M58 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M58, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M58, @P_Export);

    DECLARE @M59 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'History Report', N'Link', N'PayoutReport', N'History', N'bi bi-clock-history', 7, 1);
    SET @M59 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M59, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M59, @P_Export);

    DECLARE @M60 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Sales Comm Summary', N'Link', N'SalesCommissionReport', N'Summary', N'bi bi-percent', 8, 1);
    SET @M60 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M60, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M60, @P_Export);

    DECLARE @M61 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Sales Comm Detail', N'Link', N'SalesCommissionReport', N'Detail', N'bi bi-list-columns-reverse', 9, 1);
    SET @M61 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M61, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M61, @P_Export);

    DECLARE @M62 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M51, N'Sales Comm History', N'Link', N'SalesCommissionReport', N'History', N'bi bi-clock-history', 10, 1);
    SET @M62 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M62, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M62, @P_Export);

    DECLARE @M63 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (NULL, N'CRM', N'Collapsible', NULL, NULL, N'bi bi-headset', 5, 1);
    SET @M63 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M63, @P_View);

    DECLARE @M64 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'My Tickets', N'Link', N'HelpDeskTicket', N'MyTickets', N'bi bi-ticket-perforated', 0, 1);
    SET @M64 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M64, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M64, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M64, @P_Edit);

    DECLARE @M65 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'Ticket Management', N'Link', N'HelpDeskTicket', N'Index', N'bi bi-kanban', 1, 1);
    SET @M65 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M65, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M65, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M65, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M65, @P_Delete);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M65, @P_Approve);

    DECLARE @M66 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'My Task Log', N'Link', N'DailyTaskLog', N'Index', N'bi bi-journal-check', 2, 1);
    SET @M66 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M66, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M66, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M66, @P_Edit);

    DECLARE @M67 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'Team Task Log', N'Link', N'DailyTaskLog', N'TeamView', N'bi bi-people', 3, 1);
    SET @M67 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M67, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M67, @P_Export);

    DECLARE @M68 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'Master', N'Collapsible', NULL, NULL, N'bi bi-collection', 4, 1);
    SET @M68 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M68, @P_View);

    DECLARE @M69 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Ticket Categories', N'Link', N'TicketCategory', N'Index', N'bi bi-tags', 0, 1);
    SET @M69 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M69, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M69, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M69, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M69, @P_Delete);

    DECLARE @M70 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Ticket Sub Categories', N'Link', N'TicketSubCategory', N'Index', N'bi bi-bookmark', 1, 1);
    SET @M70 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M70, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M70, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M70, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M70, @P_Delete);

    DECLARE @M71 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Ticket Priorities', N'Link', N'TicketPriority', N'Index', N'bi bi-flag', 2, 1);
    SET @M71 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M71, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M71, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M71, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M71, @P_Delete);

    DECLARE @M72 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Task Types', N'Link', N'TaskTypeMaster', N'Index', N'bi bi-code-slash', 3, 1);
    SET @M72 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M72, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M72, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M72, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M72, @P_Delete);

    DECLARE @M73 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Task Categories', N'Link', N'TaskCategoryMaster', N'Index', N'bi bi-bookmark-star', 4, 1);
    SET @M73 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M73, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M73, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M73, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M73, @P_Delete);

    DECLARE @M74 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M68, N'Project / Module', N'Link', N'ProjectModule', N'Index', N'bi bi-folder2-open', 5, 1);
    SET @M74 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M74, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M74, @P_Create);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M74, @P_Edit);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M74, @P_Delete);

    DECLARE @M75 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M63, N'Reports', N'Collapsible', NULL, NULL, N'bi bi-bar-chart-line', 5, 1);
    SET @M75 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M75, @P_View);

    DECLARE @M76 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Analytics Dashboard', N'Link', N'TicketReports', N'Dashboard', N'bi bi-speedometer2', 0, 1);
    SET @M76 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M76, @P_View);

    DECLARE @M77 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Agent Performance', N'Link', N'TicketReports', N'AgentPerformance', N'bi bi-people', 1, 1);
    SET @M77 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M77, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M77, @P_Export);

    DECLARE @M78 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'SLA Compliance', N'Link', N'TicketReports', N'SlaCompliance', N'bi bi-shield-check', 2, 1);
    SET @M78 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M78, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M78, @P_Export);

    DECLARE @M79 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Timesheet Report', N'Link', N'TaskReport', N'Timesheet', N'bi bi-calendar3-week', 3, 1);
    SET @M79 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M79, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M79, @P_Export);

    DECLARE @M80 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Employee Productivity', N'Link', N'TaskReport', N'EmployeeProductivity', N'bi bi-graph-up-arrow', 4, 1);
    SET @M80 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M80, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M80, @P_Export);

    DECLARE @M81 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Project Effort', N'Link', N'TaskReport', N'ProjectEffort', N'bi bi-folder2-open', 5, 1);
    SET @M81 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M81, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M81, @P_Export);

    DECLARE @M82 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Payout Summary', N'Link', N'PayoutReport', N'Summary', N'bi bi-cash-stack', 6, 1);
    SET @M82 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M82, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M82, @P_Export);

    DECLARE @M83 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Payout Detail', N'Link', N'PayoutReport', N'Detail', N'bi bi-receipt', 7, 1);
    SET @M83 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M83, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M83, @P_Export);

    DECLARE @M84 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'Payout History', N'Link', N'PayoutReport', N'History', N'bi bi-clock-history', 8, 1);
    SET @M84 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M84, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M84, @P_Export);

    DECLARE @M85 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'CRM Sales Comm Summary', N'Link', N'SalesCommissionReport', N'Summary', N'bi bi-percent', 9, 1);
    SET @M85 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M85, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M85, @P_Export);

    DECLARE @M86 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'CRM Sales Comm Detail', N'Link', N'SalesCommissionReport', N'Detail', N'bi bi-receipt', 10, 1);
    SET @M86 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M86, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M86, @P_Export);

    DECLARE @M87 INT;
    INSERT INTO MenuMaster (ParentId, MenuName, MenuType, ControllerName, ActionName, IconClass, SortOrder, IsActive)
        VALUES (@M75, N'CRM Sales Comm History', N'Link', N'SalesCommissionReport', N'History', N'bi bi-clock-history', 11, 1);
    SET @M87 = SCOPE_IDENTITY();
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M87, @P_View);
    INSERT INTO MenuPermissionMap (MenuId, PermissionId) VALUES (@M87, @P_Export);

END
GO

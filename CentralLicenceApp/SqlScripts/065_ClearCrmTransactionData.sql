-- ============================================================
-- CRM Transaction Data Clear Script
-- Clears: Tickets, Messages, Attachments, Audit Logs,
--         Daily Task Logs, Time Logs, Approvals
-- Preserves: All Master data (TicketCategoryMaster,
--            TicketSubCategoryMaster, TicketPriorityMaster,
--            TaskTypeMaster, TaskCategoryMaster,
--            ProjectModuleMaster, UserMaster, etc.)
-- ============================================================
-- WARNING: This script permanently deletes all CRM transaction
--          data. Run with caution. Take a backup first.
-- ============================================================

BEGIN TRANSACTION;
BEGIN TRY

    -- Step 1: Clear Task Time Logs (FK → DailyTaskLog)
    DELETE FROM TaskTimeLog;
    PRINT 'Cleared TaskTimeLog';

    -- Step 2: Clear Task Approvals (FK → DailyTaskLog)
    DELETE FROM DailyTaskApproval;
    PRINT 'Cleared DailyTaskApproval';

    -- Step 3: Clear Daily Task Logs (FK → HelpDeskTicket)
    DELETE FROM DailyTaskLog;
    PRINT 'Cleared DailyTaskLog';

    -- Step 4: Clear Ticket Attachments (FK → TicketMessage, HelpDeskTicket)
    DELETE FROM TicketAttachment;
    PRINT 'Cleared TicketAttachment';

    -- Step 5: Clear Ticket Messages (FK → HelpDeskTicket)
    DELETE FROM TicketMessage;
    PRINT 'Cleared TicketMessage';

    -- Step 6: Clear Ticket Audit Logs (FK → HelpDeskTicket)
    DELETE FROM TicketAuditLog;
    PRINT 'Cleared TicketAuditLog';

    -- Step 7: Clear Help Desk Tickets (root transaction table)
    DELETE FROM HelpDeskTicket;
    PRINT 'Cleared HelpDeskTicket';

    -- Reset identity seeds so new records start from 1
    DBCC CHECKIDENT ('TaskTimeLog', RESEED, 0);
    DBCC CHECKIDENT ('DailyTaskApproval', RESEED, 0);
    DBCC CHECKIDENT ('DailyTaskLog', RESEED, 0);
    DBCC CHECKIDENT ('TicketAttachment', RESEED, 0);
    DBCC CHECKIDENT ('TicketMessage', RESEED, 0);
    DBCC CHECKIDENT ('TicketAuditLog', RESEED, 0);
    DBCC CHECKIDENT ('HelpDeskTicket', RESEED, 0);
    PRINT 'Identity seeds reset';

    COMMIT TRANSACTION;
    PRINT '=== CRM transaction data cleared successfully ===';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO

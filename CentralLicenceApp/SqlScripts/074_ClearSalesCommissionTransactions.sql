-- ============================================================
-- Script  : 074_ClearSalesCommissionTransactions.sql
-- Purpose : Clear ALL Sales Commission transaction data in one
--           atomic transaction. Master configuration is
--           preserved (SalesCommissionConfiguration,
--           SalesCommissionRule).
--
--           Deletion order (leaf → root, respects FK constraints):
--             1. SalesCommissionApprovalHistory (CASCADE from Batch)
--             2. SalesCommissionBatchLine       (CASCADE from Batch)
--             3. SalesCommissionBatch
--             4. SalesInvoiceAssignmentLine     (CASCADE from Assignment)
--             5. SalesInvoiceAssignment
--
--           Side-effects:
--             • All identity seeds reset to 0 (next row gets Id = 1)
--
-- ⚠  THIS IS DESTRUCTIVE — run only on test / staging databases.
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY

    -- ── 1. Commission Approval History ─────────────────────────────
    DELETE FROM dbo.SalesCommissionApprovalHistory;
    PRINT CONCAT('SalesCommissionApprovalHistory rows deleted : ', @@ROWCOUNT);

    -- ── 2. Commission Batch Lines ──────────────────────────────────
    DELETE FROM dbo.SalesCommissionBatchLine;
    PRINT CONCAT('SalesCommissionBatchLine rows deleted       : ', @@ROWCOUNT);

    -- ── 3. Commission Batches ──────────────────────────────────────
    DELETE FROM dbo.SalesCommissionBatch;
    PRINT CONCAT('SalesCommissionBatch rows deleted           : ', @@ROWCOUNT);

    -- ── 4. Invoice Assignment Lines ────────────────────────────────
    DELETE FROM dbo.SalesInvoiceAssignmentLine;
    PRINT CONCAT('SalesInvoiceAssignmentLine rows deleted     : ', @@ROWCOUNT);

    -- ── 5. Invoice Assignments ─────────────────────────────────────
    DELETE FROM dbo.SalesInvoiceAssignment;
    PRINT CONCAT('SalesInvoiceAssignment rows deleted         : ', @@ROWCOUNT);

    -- ── Reset identity seeds ───────────────────────────────────────
    DBCC CHECKIDENT ('dbo.SalesCommissionApprovalHistory', RESEED, 0);
    DBCC CHECKIDENT ('dbo.SalesCommissionBatchLine',       RESEED, 0);
    DBCC CHECKIDENT ('dbo.SalesCommissionBatch',           RESEED, 0);
    DBCC CHECKIDENT ('dbo.SalesInvoiceAssignmentLine',     RESEED, 0);
    DBCC CHECKIDENT ('dbo.SalesInvoiceAssignment',         RESEED, 0);

    COMMIT TRANSACTION;
    PRINT '✔  074_ClearSalesCommissionTransactions completed successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✘  Error — transaction rolled back. No data was changed.';
    PRINT ERROR_MESSAGE();
END CATCH;

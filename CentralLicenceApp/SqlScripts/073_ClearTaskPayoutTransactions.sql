-- ============================================================
-- Script  : 073_ClearTaskPayoutTransactions.sql
-- Purpose : Clear ALL Task Payout transaction data in one
--           atomic transaction. Master configuration is
--           preserved (PayoutConfiguration, PayoutCommissionRule).
--
--           Deletion order (leaf → root, respects FK constraints):
--             1. PayoutApprovalHistory  (CASCADE from PayoutBatch)
--             2. PayoutBatchLine        (CASCADE from PayoutBatch)
--             3. PayoutBatch
--
--           Side-effects:
--             • All identity seeds reset to 0 (next row gets Id = 1)
--
-- ⚠  THIS IS DESTRUCTIVE — run only on test / staging databases.
-- ============================================================

BEGIN TRANSACTION;

BEGIN TRY

    -- ── 1. Approval History ────────────────────────────────────────
    DELETE FROM dbo.PayoutApprovalHistory;
    PRINT CONCAT('PayoutApprovalHistory rows deleted : ', @@ROWCOUNT);

    -- ── 2. Batch Lines ─────────────────────────────────────────────
    DELETE FROM dbo.PayoutBatchLine;
    PRINT CONCAT('PayoutBatchLine rows deleted       : ', @@ROWCOUNT);

    -- ── 3. Batches ─────────────────────────────────────────────────
    DELETE FROM dbo.PayoutBatch;
    PRINT CONCAT('PayoutBatch rows deleted           : ', @@ROWCOUNT);

    -- ── Reset identity seeds ───────────────────────────────────────
    DBCC CHECKIDENT ('dbo.PayoutApprovalHistory', RESEED, 0);
    DBCC CHECKIDENT ('dbo.PayoutBatchLine',       RESEED, 0);
    DBCC CHECKIDENT ('dbo.PayoutBatch',           RESEED, 0);

    COMMIT TRANSACTION;
    PRINT '✔  073_ClearTaskPayoutTransactions completed successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '✘  Error — transaction rolled back. No data was changed.';
    PRINT ERROR_MESSAGE();
END CATCH;

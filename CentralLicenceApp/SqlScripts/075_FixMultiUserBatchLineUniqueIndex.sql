-- ============================================================
-- 075 – Fix unique index on SalesCommissionBatchLine
--       to allow the same InvoicePaymentId in multiple batches
--       (for different sales users assigned to the same invoice).
-- ============================================================

-- Drop the old unique index on InvoicePaymentId alone
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_SalesCommBatchLine_PaymentId'
      AND object_id = OBJECT_ID('dbo.SalesCommissionBatchLine')
)
BEGIN
    DROP INDEX UX_SalesCommBatchLine_PaymentId ON dbo.SalesCommissionBatchLine;
    PRINT 'Dropped UX_SalesCommBatchLine_PaymentId';
END
GO

-- Create a new unique index on (InvoicePaymentId, BatchId)
-- This still prevents duplicate payment rows within the SAME batch,
-- but allows the same payment to appear in different batches (different users).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_SalesCommBatchLine_Payment_Batch'
      AND object_id = OBJECT_ID('dbo.SalesCommissionBatchLine')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_SalesCommBatchLine_Payment_Batch
        ON dbo.SalesCommissionBatchLine (InvoicePaymentId, BatchId);
    PRINT 'Created UX_SalesCommBatchLine_Payment_Batch';
END
GO

-- ============================================================
-- 064 – Add SettlementNotRequired flag to ExpenseRequest
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ExpenseRequest' AND COLUMN_NAME = 'SettlementNotRequired'
)
BEGIN
    ALTER TABLE ExpenseRequest ADD SettlementNotRequired BIT NOT NULL DEFAULT 0;
END
GO

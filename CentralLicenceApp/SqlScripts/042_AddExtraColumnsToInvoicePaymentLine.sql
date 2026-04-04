-- ============================================================
-- Script  : 042_AddExtraColumnsToInvoicePaymentLine.sql
-- Purpose : Add CardType, CardLastFour, BankId, BankName columns
--           to InvoicePaymentLine table (idempotent - safe to re-run).
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('dbo.InvoicePaymentLine')
      AND  name      = 'CardType')
BEGIN
    ALTER TABLE dbo.InvoicePaymentLine
        ADD CardType NVARCHAR(20) NULL;
    PRINT 'Added column: CardType';
END
ELSE
    PRINT 'Column already exists: CardType';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('dbo.InvoicePaymentLine')
      AND  name      = 'CardLastFour')
BEGIN
    ALTER TABLE dbo.InvoicePaymentLine
        ADD CardLastFour NCHAR(4) NULL;
    PRINT 'Added column: CardLastFour';
END
ELSE
    PRINT 'Column already exists: CardLastFour';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('dbo.InvoicePaymentLine')
      AND  name      = 'BankId')
BEGIN
    ALTER TABLE dbo.InvoicePaymentLine
        ADD BankId INT NULL;
    PRINT 'Added column: BankId';
END
ELSE
    PRINT 'Column already exists: BankId';

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE  object_id = OBJECT_ID('dbo.InvoicePaymentLine')
      AND  name      = 'BankName')
BEGIN
    ALTER TABLE dbo.InvoicePaymentLine
        ADD BankName NVARCHAR(150) NULL;
    PRINT 'Added column: BankName';
END
ELSE
    PRINT 'Column already exists: BankName';

-- =============================================
-- Add FinancialYearId to all transaction tables
-- =============================================

-- 1. ExpenseRequest
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.ExpenseRequest ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.ExpenseRequest ADD CONSTRAINT FK_ExpenseRequest_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 2. Quotation
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Quotation') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.Quotation ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.Quotation ADD CONSTRAINT FK_Quotation_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 3. Invoice
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Invoice') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.Invoice ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.Invoice ADD CONSTRAINT FK_Invoice_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 4. InvoicePayment
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InvoicePayment') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.InvoicePayment ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.InvoicePayment ADD CONSTRAINT FK_InvoicePayment_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 5. HelpDeskTicket
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('HelpDeskTicket') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.HelpDeskTicket ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.HelpDeskTicket ADD CONSTRAINT FK_HelpDeskTicket_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 6. InvoiceRefund
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceRefund') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.InvoiceRefund ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.InvoiceRefund ADD CONSTRAINT FK_InvoiceRefund_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- 7. CreditNote
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CreditNote') AND name = 'FinancialYearId')
BEGIN
    ALTER TABLE dbo.CreditNote ADD FinancialYearId INT NULL;
    ALTER TABLE dbo.CreditNote ADD CONSTRAINT FK_CreditNote_FinancialYear
        FOREIGN KEY (FinancialYearId) REFERENCES dbo.FinancialYearMaster(Id);
END

-- Backfill existing records: assign FY based on CreatedAt date
UPDATE er
SET    er.FinancialYearId = fy.Id
FROM   ExpenseRequest er
INNER JOIN FinancialYearMaster fy ON er.CreatedAt >= fy.StartDate AND er.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  er.FinancialYearId IS NULL;

UPDATE q
SET    q.FinancialYearId = fy.Id
FROM   Quotation q
INNER JOIN FinancialYearMaster fy ON q.CreatedAt >= fy.StartDate AND q.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  q.FinancialYearId IS NULL;

UPDATE i
SET    i.FinancialYearId = fy.Id
FROM   Invoice i
INNER JOIN FinancialYearMaster fy ON i.CreatedAt >= fy.StartDate AND i.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  i.FinancialYearId IS NULL;

UPDATE p
SET    p.FinancialYearId = fy.Id
FROM   InvoicePayment p
INNER JOIN FinancialYearMaster fy ON p.CreatedAt >= fy.StartDate AND p.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  p.FinancialYearId IS NULL;

UPDATE t
SET    t.FinancialYearId = fy.Id
FROM   HelpDeskTicket t
INNER JOIN FinancialYearMaster fy ON t.CreatedAt >= fy.StartDate AND t.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  t.FinancialYearId IS NULL;

UPDATE r
SET    r.FinancialYearId = fy.Id
FROM   InvoiceRefund r
INNER JOIN FinancialYearMaster fy ON r.CreatedAt >= fy.StartDate AND r.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  r.FinancialYearId IS NULL;

UPDATE cn
SET    cn.FinancialYearId = fy.Id
FROM   CreditNote cn
INNER JOIN FinancialYearMaster fy ON cn.CreatedAt >= fy.StartDate AND cn.CreatedAt < DATEADD(DAY, 1, fy.EndDate)
WHERE  cn.FinancialYearId IS NULL;

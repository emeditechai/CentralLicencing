IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'ReimbursementStartedAt')
    ALTER TABLE dbo.ExpenseRequest ADD ReimbursementStartedAt DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'ReimbursementStartedById')
    ALTER TABLE dbo.ExpenseRequest ADD ReimbursementStartedById INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'ReimbursementRemarks')
    ALTER TABLE dbo.ExpenseRequest ADD ReimbursementRemarks NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementAmount')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementAmount DECIMAL(18,2) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementDate')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementDate DATE NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettledAt')
    ALTER TABLE dbo.ExpenseRequest ADD SettledAt DATETIME NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettledById')
    ALTER TABLE dbo.ExpenseRequest ADD SettledById INT NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementMode')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementMode NVARCHAR(30) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementReferenceNo')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementReferenceNo NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementRemarks')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementRemarks NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ExpenseRequest') AND name = 'SettlementReceiptNumber')
    ALTER TABLE dbo.ExpenseRequest ADD SettlementReceiptNumber NVARCHAR(40) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExpenseRequest_ReimbursementStartedBy')
    ALTER TABLE dbo.ExpenseRequest
    ADD CONSTRAINT FK_ExpenseRequest_ReimbursementStartedBy
    FOREIGN KEY (ReimbursementStartedById) REFERENCES dbo.UserMaster(Id);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExpenseRequest_SettledBy')
    ALTER TABLE dbo.ExpenseRequest
    ADD CONSTRAINT FK_ExpenseRequest_SettledBy
    FOREIGN KEY (SettledById) REFERENCES dbo.UserMaster(Id);
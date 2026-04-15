-- 072: Multi-user Invoice Assignment with Line-Item Commission
-- =============================================================
-- 1. Remove old unique constraint on InvoiceId (single-user)
-- 2. Add unique on (InvoiceId, SalesUserId)
-- 3. Create SalesInvoiceAssignmentLine child table
-- =============================================================

-- 1) Drop the old single-invoice unique constraint
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.SalesInvoiceAssignment')
      AND name = 'UQ_SalesInvAssign_InvoiceId'
)
BEGIN
    ALTER TABLE dbo.SalesInvoiceAssignment DROP CONSTRAINT UQ_SalesInvAssign_InvoiceId;
    PRINT 'Dropped UQ_SalesInvAssign_InvoiceId';
END
GO

-- 2) Add unique on (InvoiceId, SalesUserId) to prevent same user assigned twice
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.SalesInvoiceAssignment')
      AND name = 'UQ_SalesInvAssign_Invoice_User'
)
BEGIN
    ALTER TABLE dbo.SalesInvoiceAssignment
        ADD CONSTRAINT UQ_SalesInvAssign_Invoice_User UNIQUE (InvoiceId, SalesUserId);
    PRINT 'Added UQ_SalesInvAssign_Invoice_User';
END
GO

-- 3) Create SalesInvoiceAssignmentLine — per-line-item commission detail
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SalesInvoiceAssignmentLine')
BEGIN
    CREATE TABLE [dbo].[SalesInvoiceAssignmentLine] (
        [Id]                INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [AssignmentId]      INT             NOT NULL,
        [InvoiceLineId]     INT             NOT NULL,
        [ItemDescription]   NVARCHAR(300)   NOT NULL,
        [NetAmount]         DECIMAL(18,2)   NOT NULL,   -- Amount excluding GST
        [CommissionType]    NVARCHAR(20)    NOT NULL,   -- 'Percentage' or 'FixedAmount'
        [CommissionRate]    DECIMAL(18,2)   NOT NULL,
        [CommissionAmount]  DECIMAL(18,2)   NOT NULL,   -- Computed: Percentage → NetAmount*Rate/100; Fixed → Rate
        CONSTRAINT FK_SalesInvAssignLine_Assignment FOREIGN KEY ([AssignmentId])
            REFERENCES [SalesInvoiceAssignment]([Id]) ON DELETE CASCADE,
        CONSTRAINT FK_SalesInvAssignLine_InvoiceLine FOREIGN KEY ([InvoiceLineId])
            REFERENCES [InvoiceLine]([Id]),
        CONSTRAINT CK_SalesInvAssignLine_CommType CHECK ([CommissionType] IN ('Percentage','FixedAmount'))
    );

    CREATE NONCLUSTERED INDEX IX_SalesInvAssignLine_AssignmentId
        ON [dbo].[SalesInvoiceAssignmentLine] ([AssignmentId]);

    CREATE UNIQUE NONCLUSTERED INDEX UX_SalesInvAssignLine_Assignment_Line
        ON [dbo].[SalesInvoiceAssignmentLine] ([AssignmentId], [InvoiceLineId]);

    PRINT 'Created SalesInvoiceAssignmentLine table';
END
GO

-- 4) Backfill: for existing assignments that have no lines, we don't create lines
--    (they'll continue working; new assignments will always have lines)

-- 5) Update RateSource CHECK constraint to allow 'LineItem'
IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE name = 'CK_SalesCommBatchLine_Source'
      AND parent_object_id = OBJECT_ID('dbo.SalesCommissionBatchLine')
)
BEGIN
    ALTER TABLE dbo.SalesCommissionBatchLine DROP CONSTRAINT CK_SalesCommBatchLine_Source;
    ALTER TABLE dbo.SalesCommissionBatchLine
        ADD CONSTRAINT CK_SalesCommBatchLine_Source CHECK ([RateSource] IN ('Product','Default','LineItem'));
    PRINT 'Updated CK_SalesCommBatchLine_Source to include LineItem';
END
GO

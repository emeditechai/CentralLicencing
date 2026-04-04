-- ============================================================
-- 047 : Add TermsConditionTemplateId FK to Quotation + Invoice
--       Also expand Notes columns to NVARCHAR(MAX)
-- ============================================================

-- ── Quotation ─────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Quotation') AND name = N'TermsConditionTemplateId'
)
BEGIN
    ALTER TABLE dbo.Quotation
        ADD TermsConditionTemplateId INT NULL;

    ALTER TABLE dbo.Quotation
        ADD CONSTRAINT FK_Quotation_TermsConditionTemplate
        FOREIGN KEY (TermsConditionTemplateId)
        REFERENCES dbo.TermsConditionTemplate(Id)
        ON DELETE SET NULL;
END
GO

-- Expand Notes to NVARCHAR(MAX) if it is narrower
DECLARE @notesType_Q NVARCHAR(20);
SELECT @notesType_Q = max_length
FROM   sys.columns
WHERE  object_id = OBJECT_ID(N'dbo.Quotation') AND name = N'Notes';

IF @notesType_Q != -1  -- -1 means MAX
BEGIN
    ALTER TABLE dbo.Quotation ALTER COLUMN Notes NVARCHAR(MAX) NULL;
END
GO

-- ── Invoice ───────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Invoice') AND name = N'TermsConditionTemplateId'
)
BEGIN
    ALTER TABLE dbo.Invoice
        ADD TermsConditionTemplateId INT NULL;

    ALTER TABLE dbo.Invoice
        ADD CONSTRAINT FK_Invoice_TermsConditionTemplate
        FOREIGN KEY (TermsConditionTemplateId)
        REFERENCES dbo.TermsConditionTemplate(Id)
        ON DELETE SET NULL;
END
GO

-- Expand Notes to NVARCHAR(MAX) if it is narrower
DECLARE @notesType_I NVARCHAR(20);
SELECT @notesType_I = max_length
FROM   sys.columns
WHERE  object_id = OBJECT_ID(N'dbo.Invoice') AND name = N'Notes';

IF @notesType_I != -1
BEGIN
    ALTER TABLE dbo.Invoice ALTER COLUMN Notes NVARCHAR(MAX) NULL;
END
GO

-- ============================================================
-- 045 : Add EnableRoundOff toggle to Quotation and Invoice
-- ============================================================

-- Quotation: add EnableRoundOff + RoundOff columns
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Quotation') AND name = N'EnableRoundOff'
)
BEGIN
    ALTER TABLE dbo.Quotation
        ADD EnableRoundOff BIT          NOT NULL DEFAULT(0),
            RoundOff       DECIMAL(18,2) NOT NULL DEFAULT(0);
END
GO

-- Invoice: add EnableRoundOff column (RoundOff column already exists)
IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Invoice') AND name = N'EnableRoundOff'
)
BEGIN
    ALTER TABLE dbo.Invoice
        ADD EnableRoundOff BIT NOT NULL DEFAULT(0);
END
GO

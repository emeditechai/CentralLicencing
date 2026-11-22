-- ============================================================
-- 089 – Store billing frequency (Monthly / Quarterly / Half Yearly / Annual)
-- on quotation lines so Subscription items can print their frequency.
-- Idempotent.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationLine') AND name = 'BillingFrequency')
BEGIN
    ALTER TABLE dbo.QuotationLine ADD BillingFrequency NVARCHAR(20) NULL;
END
GO

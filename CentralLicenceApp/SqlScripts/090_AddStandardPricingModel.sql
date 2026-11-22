-- ============================================================
-- 090 – Add "Standard" to PricingModelMaster. Idempotent.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM dbo.PricingModelMaster WHERE ModelName = N'Standard')
BEGIN
    INSERT INTO dbo.PricingModelMaster (ModelName, Description, IsActive, CreatedAt)
    VALUES (N'Standard', N'Standard pricing model', 1, GETDATE());
END
GO

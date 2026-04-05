-- Script 049: Add SubscriptionStartDate to ClientPurchasedProduct
-- This column stores the explicit subscription start date so the renewal calculator
-- uses the actual subscription start rather than the DB insert timestamp (CreatedAt).
-- For existing rows, NULL is fine — the code falls back to CreatedAt via COALESCE.

IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns
    WHERE  object_id = OBJECT_ID('dbo.ClientPurchasedProduct')
      AND  name      = 'SubscriptionStartDate'
)
BEGIN
    ALTER TABLE [dbo].[ClientPurchasedProduct]
    ADD [SubscriptionStartDate] DATETIME NULL;
END

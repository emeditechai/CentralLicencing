-- Script 050: Add LastRenewedAt to ClientPurchasedProduct
-- Stores the timestamp when a Core Member last clicked "Mark as Renewed" on the dashboard.
-- The subscription reminder query uses this to suppress a row for the current cycle after
-- it has already been acknowledged.

IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns
    WHERE  object_id = OBJECT_ID('dbo.ClientPurchasedProduct')
      AND  name      = 'LastRenewedAt'
)
BEGIN
    ALTER TABLE [dbo].[ClientPurchasedProduct]
    ADD [LastRenewedAt] DATETIME NULL;
END

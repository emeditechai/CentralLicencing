IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct')
      AND name = 'InvoiceNo'
)
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct
        ADD InvoiceNo NVARCHAR(100) NULL;
END;

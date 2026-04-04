-- 039: Add CancelRemarks column to Invoice table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Invoice' AND COLUMN_NAME = 'CancelRemarks'
)
BEGIN
    ALTER TABLE dbo.Invoice ADD CancelRemarks NVARCHAR(1000) NULL;
END

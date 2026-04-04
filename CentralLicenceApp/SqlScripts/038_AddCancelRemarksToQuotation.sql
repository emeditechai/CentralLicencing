-- 038: Add CancelRemarks column to Quotation table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Quotation' AND COLUMN_NAME = 'CancelRemarks'
)
BEGIN
    ALTER TABLE dbo.Quotation ADD CancelRemarks NVARCHAR(1000) NULL;
END

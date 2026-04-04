-- Add GST columns to QuotationLine
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationLine') AND name = 'GstPercent')
    ALTER TABLE dbo.QuotationLine ADD GstPercent  DECIMAL(5,2)  NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationLine') AND name = 'CgstAmount')
    ALTER TABLE dbo.QuotationLine ADD CgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationLine') AND name = 'SgstAmount')
    ALTER TABLE dbo.QuotationLine ADD SgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.QuotationLine') AND name = 'IgstAmount')
    ALTER TABLE dbo.QuotationLine ADD IgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);

-- Add GST totals to Quotation header
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Quotation') AND name = 'TotalCgst')
    ALTER TABLE dbo.Quotation ADD TotalCgst  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Quotation') AND name = 'TotalSgst')
    ALTER TABLE dbo.Quotation ADD TotalSgst  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Quotation') AND name = 'TotalIgst')
    ALTER TABLE dbo.Quotation ADD TotalIgst  DECIMAL(18,2) NOT NULL DEFAULT(0);

-- Add GST columns to InvoiceLine
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceLine') AND name = 'GstPercent')
    ALTER TABLE dbo.InvoiceLine ADD GstPercent  DECIMAL(5,2)  NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceLine') AND name = 'CgstAmount')
    ALTER TABLE dbo.InvoiceLine ADD CgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceLine') AND name = 'SgstAmount')
    ALTER TABLE dbo.InvoiceLine ADD SgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.InvoiceLine') AND name = 'IgstAmount')
    ALTER TABLE dbo.InvoiceLine ADD IgstAmount  DECIMAL(18,2) NOT NULL DEFAULT(0);

-- Add GST totals to Invoice header
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Invoice') AND name = 'TotalCgst')
    ALTER TABLE dbo.Invoice ADD TotalCgst  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Invoice') AND name = 'TotalSgst')
    ALTER TABLE dbo.Invoice ADD TotalSgst  DECIMAL(18,2) NOT NULL DEFAULT(0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Invoice') AND name = 'TotalIgst')
    ALTER TABLE dbo.Invoice ADD TotalIgst  DECIMAL(18,2) NOT NULL DEFAULT(0);

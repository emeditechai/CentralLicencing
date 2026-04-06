-- ============================================================
-- 052: Invoice Payment Void + Refund support
-- ============================================================

-- 1. Add void-tracking columns to InvoicePayment
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('InvoicePayment') AND name = 'IsVoided')
BEGIN
    ALTER TABLE InvoicePayment
        ADD IsVoided    BIT            NOT NULL DEFAULT 0,
            VoidedAt    DATETIME       NULL,
            VoidedBy    NVARCHAR(100)  NULL,
            VoidRemarks NVARCHAR(500)  NULL;
END;

-- 2. Create InvoiceRefund table
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'U' AND name = 'InvoiceRefund')
BEGIN
    CREATE TABLE InvoiceRefund (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        RefundNo        NVARCHAR(50)   NOT NULL,
        PaymentId       INT            NOT NULL REFERENCES InvoicePayment(Id),
        InvoiceId       INT            NOT NULL REFERENCES Invoice(Id),
        InvoiceNo       NVARCHAR(50)   NOT NULL,
        PartyId         INT            NOT NULL,
        PartyName       NVARCHAR(200)  NOT NULL,
        RefundDate      DATE           NOT NULL,
        Amount          DECIMAL(18,2)  NOT NULL,
        PaymentModeId   INT            NOT NULL,
        PaymentModeName NVARCHAR(100)  NOT NULL,
        ReferenceNo     NVARCHAR(200)  NULL,
        Remarks         NVARCHAR(500)  NULL,
        CreatedBy       NVARCHAR(100)  NULL,
        CreatedAt       DATETIME       NOT NULL DEFAULT GETDATE()
    );
END;

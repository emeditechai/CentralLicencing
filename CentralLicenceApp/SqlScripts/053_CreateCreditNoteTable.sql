-- ============================================================
-- 053: Credit Note table
-- A Credit Note is issued to a party after a refund is processed.
-- It formally documents the credit owed / cash returned.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'U' AND name = 'CreditNote')
BEGIN
    CREATE TABLE CreditNote (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        CreditNoteNo    NVARCHAR(50)    NOT NULL,
        RefundId        INT             NOT NULL REFERENCES InvoiceRefund(Id),
        RefundNo        NVARCHAR(50)    NOT NULL,
        PaymentId       INT             NOT NULL REFERENCES InvoicePayment(Id),
        ReceiptNo       NVARCHAR(50)    NOT NULL,
        InvoiceId       INT             NOT NULL REFERENCES Invoice(Id),
        InvoiceNo       NVARCHAR(50)    NOT NULL,
        PartyId         INT             NOT NULL,
        PartyName       NVARCHAR(200)   NOT NULL,
        PartyAddress    NVARCHAR(500)   NULL,
        PartyGSTINNo    NVARCHAR(50)    NULL,
        PartyPANNo      NVARCHAR(50)    NULL,
        PartyContactPerson NVARCHAR(200) NULL,
        PartyMobile     NVARCHAR(50)    NULL,
        CreditNoteDate  DATE            NOT NULL,
        Amount          DECIMAL(18,2)   NOT NULL,
        PaymentModeId   INT             NOT NULL,
        PaymentModeName NVARCHAR(100)   NOT NULL,
        ReferenceNo     NVARCHAR(200)   NULL,
        Reason          NVARCHAR(500)   NULL,
        CreatedBy       NVARCHAR(100)   NULL,
        CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE()
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InvoicePayment')
BEGIN
    CREATE TABLE dbo.InvoicePayment (
        Id              INT            IDENTITY(1,1) NOT NULL,
        ReceiptNo       NVARCHAR(30)   NOT NULL,
        InvoiceId       INT            NOT NULL,
        InvoiceNo       NVARCHAR(30)   NOT NULL,
        PartyId         INT            NOT NULL,
        PartyName       NVARCHAR(200)  NOT NULL,
        PaymentDate     DATE           NOT NULL,
        TotalAmountPaid DECIMAL(18,2)  NOT NULL DEFAULT(0),
        Notes           NVARCHAR(500)  NULL,
        CreatedBy       NVARCHAR(100)  NULL,
        CreatedAt       DATETIME       NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_InvoicePayment PRIMARY KEY (Id),
        CONSTRAINT FK_IP_Invoice FOREIGN KEY (InvoiceId)
            REFERENCES dbo.Invoice(Id)
    );
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InvoicePaymentLine')
BEGIN
    CREATE TABLE dbo.InvoicePaymentLine (
        Id              INT            IDENTITY(1,1) NOT NULL,
        PaymentId       INT            NOT NULL,
        PaymentModeId   INT            NOT NULL,
        PaymentModeName NVARCHAR(100)  NOT NULL,
        Amount          DECIMAL(18,2)  NOT NULL,
        ReferenceNo     NVARCHAR(100)  NULL,
        CONSTRAINT PK_InvoicePaymentLine PRIMARY KEY (Id),
        CONSTRAINT FK_IPL_Payment FOREIGN KEY (PaymentId)
            REFERENCES dbo.InvoicePayment(Id) ON DELETE CASCADE
    );
END

-- ===================================================
-- Quotation header
-- ===================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Quotation')
BEGIN
    CREATE TABLE dbo.Quotation (
        Id                   INT             IDENTITY(1,1) NOT NULL,
        QuotationNo          VARCHAR(30)     NOT NULL,
        QuotationDate        DATE            NOT NULL,
        ValidUntilDate       DATE            NULL,
        PartyId              INT             NOT NULL,
        PartyName            NVARCHAR(150)   NOT NULL,
        PartyAddress         NVARCHAR(300)   NULL,
        PartyGSTINNo         VARCHAR(20)     NULL,
        PartyPANNo           VARCHAR(10)     NULL,
        PartyContactPerson   NVARCHAR(100)   NULL,
        PartyMobile          VARCHAR(20)     NULL,
        Notes                NVARCHAR(500)   NULL,
        TermsAndConditions   NVARCHAR(1000)  NULL,
        SubTotal             DECIMAL(18,2)   NOT NULL DEFAULT(0),
        TotalAmount          DECIMAL(18,2)   NOT NULL DEFAULT(0),
        Status               VARCHAR(20)     NOT NULL DEFAULT('Draft'),
        CreatedBy            NVARCHAR(100)   NULL,
        CreatedAt            DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_Quotation PRIMARY KEY (Id)
    );
END;

-- ===================================================
-- Quotation line items
-- ===================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'QuotationLine')
BEGIN
    CREATE TABLE dbo.QuotationLine (
        Id               INT             IDENTITY(1,1) NOT NULL,
        QuotationId      INT             NOT NULL,
        SNo              INT             NOT NULL,
        ItemDescription  NVARCHAR(300)   NOT NULL,
        PlanName         NVARCHAR(100)   NULL,
        Type             NVARCHAR(50)    NULL,
        Qty              INT             NOT NULL DEFAULT(1),
        Rate             DECIMAL(18,2)   NOT NULL DEFAULT(0),
        DiscountPercent  DECIMAL(5,2)    NOT NULL DEFAULT(0),
        DiscountAmount   DECIMAL(18,2)   NOT NULL DEFAULT(0),
        Amount           DECIMAL(18,2)   NOT NULL DEFAULT(0),
        CONSTRAINT PK_QuotationLine PRIMARY KEY (Id),
        CONSTRAINT FK_QuotationLine_Quotation FOREIGN KEY (QuotationId) REFERENCES dbo.Quotation(Id) ON DELETE CASCADE
    );
END;

-- ===================================================
-- Invoice header
-- ===================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Invoice')
BEGIN
    CREATE TABLE dbo.Invoice (
        Id                   INT             IDENTITY(1,1) NOT NULL,
        InvoiceNo            VARCHAR(30)     NOT NULL,
        InvoiceDate          DATE            NOT NULL,
        DueDate              DATE            NULL,
        QuotationId          INT             NULL,
        QuotationNo          VARCHAR(30)     NULL,
        PartyId              INT             NOT NULL,
        PartyName            NVARCHAR(150)   NOT NULL,
        PartyAddress         NVARCHAR(300)   NULL,
        PartyGSTINNo         VARCHAR(20)     NULL,
        PartyPANNo           VARCHAR(10)     NULL,
        PartyContactPerson   NVARCHAR(100)   NULL,
        PartyMobile          VARCHAR(20)     NULL,
        Notes                NVARCHAR(500)   NULL,
        TermsAndConditions   NVARCHAR(1000)  NULL,
        SubTotal             DECIMAL(18,2)   NOT NULL DEFAULT(0),
        RoundOff             DECIMAL(18,2)   NOT NULL DEFAULT(0),
        TotalAmount          DECIMAL(18,2)   NOT NULL DEFAULT(0),
        ReceivedAmount       DECIMAL(18,2)   NOT NULL DEFAULT(0),
        PreviousBalance      DECIMAL(18,2)   NOT NULL DEFAULT(0),
        Status               VARCHAR(20)     NOT NULL DEFAULT('Draft'),
        CreatedBy            NVARCHAR(100)   NULL,
        CreatedAt            DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_Invoice PRIMARY KEY (Id)
    );
END;

-- ===================================================
-- Invoice line items
-- ===================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'InvoiceLine')
BEGIN
    CREATE TABLE dbo.InvoiceLine (
        Id               INT             IDENTITY(1,1) NOT NULL,
        InvoiceId        INT             NOT NULL,
        SNo              INT             NOT NULL,
        ItemDescription  NVARCHAR(300)   NOT NULL,
        PlanName         NVARCHAR(100)   NULL,
        Type             NVARCHAR(50)    NULL,
        Qty              INT             NOT NULL DEFAULT(1),
        Rate             DECIMAL(18,2)   NOT NULL DEFAULT(0),
        DiscountPercent  DECIMAL(5,2)    NOT NULL DEFAULT(0),
        DiscountAmount   DECIMAL(18,2)   NOT NULL DEFAULT(0),
        Amount           DECIMAL(18,2)   NOT NULL DEFAULT(0),
        CONSTRAINT PK_InvoiceLine PRIMARY KEY (Id),
        CONSTRAINT FK_InvoiceLine_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(Id) ON DELETE CASCADE
    );
END;

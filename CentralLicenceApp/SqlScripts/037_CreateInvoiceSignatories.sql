-- 037: Create InvoiceSignatories junction table
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'InvoiceSignatories'
)
BEGIN
    CREATE TABLE dbo.InvoiceSignatories (
        Id        INT          IDENTITY(1,1) NOT NULL,
        InvoiceId INT          NOT NULL,
        UserId    INT          NOT NULL,
        SortOrder INT          NOT NULL DEFAULT(0),
        CONSTRAINT PK_InvoiceSignatories PRIMARY KEY (Id),
        CONSTRAINT FK_IS_Invoice FOREIGN KEY (InvoiceId) REFERENCES dbo.Invoice(Id) ON DELETE CASCADE,
        CONSTRAINT FK_IS_User    FOREIGN KEY (UserId)    REFERENCES dbo.UserMaster(Id)
    );
END

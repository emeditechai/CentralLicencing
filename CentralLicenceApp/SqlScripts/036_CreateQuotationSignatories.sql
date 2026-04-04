-- 036: Create QuotationSignatories junction table
IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = 'QuotationSignatories'
)
BEGIN
    CREATE TABLE dbo.QuotationSignatories (
        Id          INT          IDENTITY(1,1) NOT NULL,
        QuotationId INT          NOT NULL,
        UserId      INT          NOT NULL,
        SortOrder   INT          NOT NULL DEFAULT(0),
        CONSTRAINT PK_QuotationSignatories PRIMARY KEY (Id),
        CONSTRAINT FK_QS_Quotation FOREIGN KEY (QuotationId) REFERENCES dbo.Quotation(Id) ON DELETE CASCADE,
        CONSTRAINT FK_QS_User     FOREIGN KEY (UserId)      REFERENCES dbo.UserMaster(Id)
    );
END

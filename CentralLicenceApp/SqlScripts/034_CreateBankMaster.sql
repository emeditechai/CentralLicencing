-- Create BankMaster table
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'BankMaster')
BEGIN
    CREATE TABLE dbo.BankMaster (
        Id             INT            IDENTITY(1,1) NOT NULL,
        BankName       NVARCHAR(150)  NOT NULL,
        AccountNumber  NVARCHAR(30)   NOT NULL,
        BranchName     NVARCHAR(150)  NOT NULL,
        IFSCCode       NVARCHAR(11)   NOT NULL,
        UpiId          NVARCHAR(50)   NULL,
        UpiHolderName  NVARCHAR(150)  NULL,
        IsPrimary      BIT            NOT NULL DEFAULT(0),
        IsActive       BIT            NOT NULL DEFAULT(1),
        CreatedAt      DATETIME       NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_BankMaster PRIMARY KEY (Id)
    );
END

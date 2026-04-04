IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PartyMaster')
BEGIN
    CREATE TABLE dbo.PartyMaster (
        Id            INT             IDENTITY(1,1) NOT NULL,
        PartyName     NVARCHAR(150)   NOT NULL,
        ContactPerson NVARCHAR(100)   NULL,
        Mobile        VARCHAR(20)     NULL,
        Email         NVARCHAR(100)   NULL,
        Address       NVARCHAR(300)   NULL,
        GSTINNo       VARCHAR(20)     NULL,
        PANNo         VARCHAR(10)     NULL,
        IsActive      BIT             NOT NULL DEFAULT(1),
        CreatedAt     DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_PartyMaster PRIMARY KEY (Id)
    );
END;

-- 067: Create TicketCannedResponse table for canned reply templates
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TicketCannedResponse')
BEGIN
    CREATE TABLE TicketCannedResponse (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Title       NVARCHAR(200) NOT NULL,
        Content     NVARCHAR(MAX) NOT NULL,
        CreatedById INT NOT NULL,
        IsGlobal    BIT NOT NULL DEFAULT 0,
        IsActive    BIT NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2 NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TicketCannedResponse_User FOREIGN KEY (CreatedById) REFERENCES UserMaster(Id)
    );

    CREATE NONCLUSTERED INDEX IX_TicketCannedResponse_Active
        ON TicketCannedResponse (IsActive, IsGlobal, CreatedById);
END
GO

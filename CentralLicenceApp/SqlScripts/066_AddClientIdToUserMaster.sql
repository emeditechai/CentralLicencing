-- 066: Add ClientId column to UserMaster (FK to PartyMaster)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'ClientId')
BEGIN
    ALTER TABLE UserMaster ADD ClientId INT NULL;

    ALTER TABLE UserMaster
        ADD CONSTRAINT FK_UserMaster_PartyMaster
        FOREIGN KEY (ClientId) REFERENCES PartyMaster(Id);
END
GO

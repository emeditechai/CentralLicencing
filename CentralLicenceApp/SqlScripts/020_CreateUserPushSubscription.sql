IF OBJECT_ID('dbo.UserPushSubscription', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserPushSubscription
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Endpoint NVARCHAR(1000) NOT NULL,
        P256dh NVARCHAR(300) NOT NULL,
        Auth NVARCHAR(200) NOT NULL,
        UserAgent NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_UserPushSubscription_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_UserPushSubscription_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_UserPushSubscription_UpdatedAt DEFAULT(GETDATE()),
        CONSTRAINT FK_UserPushSubscription_UserMaster FOREIGN KEY (UserId) REFERENCES dbo.UserMaster(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserPushSubscription_Endpoint UNIQUE (Endpoint)
    );

    CREATE INDEX IX_UserPushSubscription_UserId ON dbo.UserPushSubscription(UserId, IsActive, UpdatedAt DESC);
END
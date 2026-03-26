IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanyTypeMaster')
BEGIN
    CREATE TABLE [dbo].[CompanyTypeMaster]
    (
        [Id]        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TypeName]  NVARCHAR(100) NOT NULL,
        [IsActive]  BIT NOT NULL CONSTRAINT [DF_CompanyTypeMaster_IsActive] DEFAULT 1,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_CompanyTypeMaster_CreatedAt] DEFAULT GETDATE(),
        CONSTRAINT [UQ_CompanyTypeMaster_TypeName] UNIQUE ([TypeName])
    );
END;

IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'Private')
    INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('Private', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'LLP')
    INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('LLP', 1, GETDATE());

IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'Services')
    INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('Services', 1, GETDATE());

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CompanySettings')
BEGIN
    CREATE TABLE [dbo].[CompanySettings]
    (
        [Id]              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CompanyCode]     NVARCHAR(50) NOT NULL,
        [CompanyTypeId]   INT NOT NULL,
        [CompanyName]     NVARCHAR(200) NOT NULL,
        [Country]         NVARCHAR(100) NULL,
        [State]           NVARCHAR(100) NULL,
        [District]        NVARCHAR(100) NULL,
        [City]            NVARCHAR(100) NULL,
        [Address]         NVARCHAR(500) NULL,
        [Website]         NVARCHAR(200) NULL,
        [EmailId]         NVARCHAR(200) NULL,
        [ContactNo]       NVARCHAR(30) NULL,
        [Pincode]         NVARCHAR(20) NULL,
        [GSTCode]         NVARCHAR(50) NULL,
        [PANCard]         NVARCHAR(50) NULL,
        [IsParentCompany] BIT NOT NULL CONSTRAINT [DF_CompanySettings_IsParentCompany] DEFAULT 0,
        [CompanyLogoPath] NVARCHAR(300) NULL,
        [IsActive]        BIT NOT NULL CONSTRAINT [DF_CompanySettings_IsActive] DEFAULT 1,
        [CreatedAt]       DATETIME NOT NULL CONSTRAINT [DF_CompanySettings_CreatedAt] DEFAULT GETDATE(),
        CONSTRAINT [FK_CompanySettings_CompanyTypeMaster] FOREIGN KEY ([CompanyTypeId]) REFERENCES [dbo].[CompanyTypeMaster]([Id]),
        CONSTRAINT [UQ_CompanySettings_CompanyCode] UNIQUE ([CompanyCode])
    );
END;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'Website')
    ALTER TABLE [dbo].[CompanySettings] ADD [Website] NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'EmailId')
    ALTER TABLE [dbo].[CompanySettings] ADD [EmailId] NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'ContactNo')
    ALTER TABLE [dbo].[CompanySettings] ADD [ContactNo] NVARCHAR(30) NULL;
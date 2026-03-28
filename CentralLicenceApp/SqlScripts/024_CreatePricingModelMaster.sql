IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PricingModelMaster')
BEGIN
    CREATE TABLE dbo.PricingModelMaster
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ModelName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_PricingModelMaster_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_PricingModelMaster_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_PricingModelMaster_ModelName'
      AND object_id = OBJECT_ID('dbo.PricingModelMaster')
)
BEGIN
    CREATE UNIQUE INDEX UX_PricingModelMaster_ModelName
        ON dbo.PricingModelMaster (ModelName);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PricingModelMaster WHERE ModelName = 'Basic')
BEGIN
    INSERT INTO dbo.PricingModelMaster (ModelName, Description, IsActive, CreatedAt)
    VALUES ('Basic', 'Entry-level pricing model', 1, GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PricingModelMaster WHERE ModelName = 'Gold')
BEGIN
    INSERT INTO dbo.PricingModelMaster (ModelName, Description, IsActive, CreatedAt)
    VALUES ('Gold', 'Advanced pricing model', 1, GETDATE());
END;

IF NOT EXISTS (SELECT 1 FROM dbo.PricingModelMaster WHERE ModelName = 'Premium')
BEGIN
    INSERT INTO dbo.PricingModelMaster (ModelName, Description, IsActive, CreatedAt)
    VALUES ('Premium', 'Highest-tier pricing model', 1, GETDATE());
END;
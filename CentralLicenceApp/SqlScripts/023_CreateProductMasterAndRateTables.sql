IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductMaster')
BEGIN
    CREATE TABLE dbo.ProductMaster
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductCode NVARCHAR(50) NOT NULL,
        ProductName NVARCHAR(150) NOT NULL,
        ProductType NVARCHAR(50) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ProductMaster_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductMaster_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'Id'
)
BEGIN
    THROW 50003, 'ProductMaster table exists but does not contain Id column. Fix the legacy table before running this migration.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'ProductCode'
)
BEGIN
    ALTER TABLE dbo.ProductMaster
    ADD ProductCode NVARCHAR(50) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'ProductName'
)
BEGIN
    ALTER TABLE dbo.ProductMaster
    ADD ProductName NVARCHAR(150) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'ProductType'
)
BEGIN
    ALTER TABLE dbo.ProductMaster
    ADD ProductType NVARCHAR(50) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'IsActive'
)
BEGIN
    ALTER TABLE dbo.ProductMaster
    ADD IsActive BIT NOT NULL CONSTRAINT DF_ProductMaster_IsActive DEFAULT (1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductMaster')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE dbo.ProductMaster
    ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductMaster_CreatedAt DEFAULT (GETDATE());
END;

EXEC(N'
UPDATE dbo.ProductMaster
SET ProductCode = ''PRD-'' + RIGHT(''00000'' + CAST(Id AS VARCHAR(5)), 5)
WHERE NULLIF(LTRIM(RTRIM(ProductCode)), '''') IS NULL;

UPDATE dbo.ProductMaster
SET ProductName = ProductCode
WHERE NULLIF(LTRIM(RTRIM(ProductName)), '''') IS NULL;

UPDATE dbo.ProductMaster
SET ProductType = ''Healthcare''
WHERE NULLIF(LTRIM(RTRIM(ProductType)), '''') IS NULL;

ALTER TABLE dbo.ProductMaster
ALTER COLUMN ProductCode NVARCHAR(50) NOT NULL;

ALTER TABLE dbo.ProductMaster
ALTER COLUMN ProductName NVARCHAR(150) NOT NULL;

ALTER TABLE dbo.ProductMaster
ALTER COLUMN ProductType NVARCHAR(50) NOT NULL;
');

IF NOT EXISTS (
    SELECT 1
    FROM sys.key_constraints kc
    INNER JOIN sys.index_columns ic
        ON ic.object_id = kc.parent_object_id
       AND ic.index_id = kc.unique_index_id
    INNER JOIN sys.columns c
        ON c.object_id = ic.object_id
       AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID('dbo.ProductMaster')
      AND kc.type IN ('PK', 'UQ')
    GROUP BY kc.name
    HAVING COUNT(*) = 1 AND MAX(c.name) = 'Id'
)
BEGIN
    EXEC(N'
    IF EXISTS (SELECT 1 FROM dbo.ProductMaster WHERE Id IS NULL)
    BEGIN
        THROW 50004, ''ProductMaster contains NULL Id values. Fix those rows before running this migration.'', 1;
    END;

    IF EXISTS (
        SELECT Id
        FROM dbo.ProductMaster
        GROUP BY Id
        HAVING COUNT(1) > 1
    )
    BEGIN
        THROW 50005, ''ProductMaster contains duplicate Id values. Fix those rows before running this migration.'', 1;
    END;

    ALTER TABLE dbo.ProductMaster
    ADD CONSTRAINT UQ_ProductMaster_Id UNIQUE (Id);
    ');
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductMaster_ProductCode'
      AND object_id = OBJECT_ID('dbo.ProductMaster')
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX UX_ProductMaster_ProductCode ON dbo.ProductMaster (ProductCode);');
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductRate')
BEGIN
    CREATE TABLE dbo.ProductRate
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductId INT NOT NULL,
        PricingModel NVARCHAR(50) NOT NULL,
        BillingModel NVARCHAR(20) NOT NULL CONSTRAINT DF_ProductRate_BillingModel DEFAULT ('One Time'),
        BillingFrequency NVARCHAR(20) NOT NULL CONSTRAINT DF_ProductRate_BillingFrequency DEFAULT (''),
        ProductSpecification NVARCHAR(500) NULL,
        Features NVARCHAR(2000) NULL,
        Rate DECIMAL(18,2) NOT NULL,
        AmcCalculationType NVARCHAR(20) NOT NULL CONSTRAINT DF_ProductRate_AmcCalculationType DEFAULT ('Percentage'),
        AmcPercentage DECIMAL(18,4) NOT NULL CONSTRAINT DF_ProductRate_AmcPercentage DEFAULT (0),
        AmcAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_ProductRate_AmcAmount DEFAULT (0),
        IsActive BIT NOT NULL CONSTRAINT DF_ProductRate_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductRate_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'ProductId'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD ProductId INT NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'PricingModel'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD PricingModel NVARCHAR(50) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'BillingModel'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD BillingModel NVARCHAR(20) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'BillingFrequency'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD BillingFrequency NVARCHAR(20) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'ProductSpecification'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD ProductSpecification NVARCHAR(500) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'Features'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD Features NVARCHAR(2000) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'Rate'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD Rate DECIMAL(18,2) NOT NULL CONSTRAINT DF_ProductRate_Rate DEFAULT (0);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'AmcCalculationType'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD AmcCalculationType NVARCHAR(20) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'AmcPercentage'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD AmcPercentage DECIMAL(18,4) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'AmcAmount'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD AmcAmount DECIMAL(18,2) NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'IsActive'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD IsActive BIT NOT NULL CONSTRAINT DF_ProductRate_IsActive DEFAULT (1);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ProductRate')
      AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE dbo.ProductRate
    ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductRate_CreatedAt DEFAULT (GETDATE());
END;

EXEC(N'
UPDATE dbo.ProductRate
SET PricingModel = ''Basic''
WHERE NULLIF(LTRIM(RTRIM(PricingModel)), '''') IS NULL;

UPDATE dbo.ProductRate
SET BillingModel = ''One Time''
WHERE NULLIF(LTRIM(RTRIM(BillingModel)), '''') IS NULL;

UPDATE dbo.ProductRate
SET BillingFrequency = ''''
WHERE BillingFrequency IS NULL;

UPDATE dbo.ProductRate
SET AmcCalculationType = ''Percentage''
WHERE NULLIF(LTRIM(RTRIM(AmcCalculationType)), '''') IS NULL;

UPDATE dbo.ProductRate
SET AmcPercentage = 0
WHERE AmcPercentage IS NULL;

UPDATE dbo.ProductRate
SET AmcAmount = 0
WHERE AmcAmount IS NULL;

IF EXISTS (SELECT 1 FROM dbo.ProductRate WHERE ProductId IS NULL)
BEGIN
    THROW 50001, ''ProductRate contains rows with NULL ProductId. Fix those rows before running this migration.'', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.ProductRate pr
    LEFT JOIN dbo.ProductMaster pm ON pm.Id = pr.ProductId
    WHERE pm.Id IS NULL
)
BEGIN
    THROW 50002, ''ProductRate contains rows pointing to missing ProductMaster records. Fix those rows before running this migration.'', 1;
END;

ALTER TABLE dbo.ProductRate
ALTER COLUMN ProductId INT NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN PricingModel NVARCHAR(50) NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN BillingModel NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN BillingFrequency NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN AmcCalculationType NVARCHAR(20) NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN AmcPercentage DECIMAL(18,4) NOT NULL;

ALTER TABLE dbo.ProductRate
ALTER COLUMN AmcAmount DECIMAL(18,2) NOT NULL;
');

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ProductRate_ProductMaster'
)
BEGIN
    EXEC(N'
    ALTER TABLE dbo.ProductRate
    WITH CHECK ADD CONSTRAINT FK_ProductRate_ProductMaster
    FOREIGN KEY (ProductId) REFERENCES dbo.ProductMaster(Id);
    ');
END;

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductRate_ProductId_PricingModel'
      AND object_id = OBJECT_ID('dbo.ProductRate')
)
BEGIN
    DROP INDEX UX_ProductRate_ProductId_PricingModel ON dbo.ProductRate;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductRate_ProductId_PricingModel_BillingModel_BillingFrequency'
      AND object_id = OBJECT_ID('dbo.ProductRate')
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX UX_ProductRate_ProductId_PricingModel_BillingModel_BillingFrequency ON dbo.ProductRate (ProductId, PricingModel, BillingModel, BillingFrequency);');
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ClientPurchasedProduct')
BEGIN
    CREATE TABLE dbo.ClientPurchasedProduct
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClientDetailsId INT NOT NULL,
        ClientCode VARCHAR(20) NOT NULL,
        ProductId INT NOT NULL,
        ProductRateId INT NOT NULL,
        ProductCode NVARCHAR(50) NOT NULL,
        ProductName NVARCHAR(150) NOT NULL,
        PricingModel NVARCHAR(50) NOT NULL,
        BillingModel NVARCHAR(20) NOT NULL CONSTRAINT DF_ClientPurchasedProduct_BillingModel DEFAULT ('One Time'),
        BillingFrequency NVARCHAR(20) NOT NULL CONSTRAINT DF_ClientPurchasedProduct_BillingFrequency DEFAULT (''),
        BasePrice DECIMAL(18,2) NOT NULL,
        AmcCalculationType NVARCHAR(20) NOT NULL,
        AmcPercentage DECIMAL(18,4) NOT NULL,
        AmcAmount DECIMAL(18,2) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ClientPurchasedProduct_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ClientPurchasedProduct_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.ClientDetails')
      AND name = 'ID'
)
BEGIN
    THROW 50006, 'ClientDetails table exists but does not contain ID column. Fix the legacy table before running this migration.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.key_constraints kc
    INNER JOIN sys.index_columns ic
        ON ic.object_id = kc.parent_object_id
       AND ic.index_id = kc.unique_index_id
    INNER JOIN sys.columns c
        ON c.object_id = ic.object_id
       AND c.column_id = ic.column_id
    WHERE kc.parent_object_id = OBJECT_ID('dbo.ClientDetails')
      AND kc.type IN ('PK', 'UQ')
    GROUP BY kc.name
    HAVING COUNT(*) = 1 AND MAX(c.name) = 'ID'
)
BEGIN
    EXEC(N'
    IF EXISTS (SELECT 1 FROM dbo.ClientDetails WHERE ID IS NULL)
    BEGIN
        THROW 50007, ''ClientDetails contains NULL ID values. Fix those rows before running this migration.'', 1;
    END;

    IF EXISTS (
        SELECT ID
        FROM dbo.ClientDetails
        GROUP BY ID
        HAVING COUNT(1) > 1
    )
    BEGIN
        THROW 50008, ''ClientDetails contains duplicate ID values. Fix those rows before running this migration.'', 1;
    END;

    ALTER TABLE dbo.ClientDetails
    ADD CONSTRAINT UQ_ClientDetails_ID UNIQUE (ID);
    ');
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'ClientCode')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD ClientCode VARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'ProductId')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD ProductId INT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'ProductRateId')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD ProductRateId INT NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'ProductCode')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD ProductCode NVARCHAR(50) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'ProductName')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD ProductName NVARCHAR(150) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'PricingModel')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD PricingModel NVARCHAR(50) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'BillingModel')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD BillingModel NVARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'BillingFrequency')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD BillingFrequency NVARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'BasePrice')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD BasePrice DECIMAL(18,2) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'AmcCalculationType')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD AmcCalculationType NVARCHAR(20) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'AmcPercentage')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD AmcPercentage DECIMAL(18,4) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'AmcAmount')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD AmcAmount DECIMAL(18,2) NULL;
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'IsActive')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD IsActive BIT NOT NULL CONSTRAINT DF_ClientPurchasedProduct_IsActive DEFAULT (1);
END;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ClientPurchasedProduct') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE dbo.ClientPurchasedProduct ADD CreatedAt DATETIME NOT NULL CONSTRAINT DF_ClientPurchasedProduct_CreatedAt DEFAULT (GETDATE());
END;

EXEC(N'
UPDATE dbo.ClientPurchasedProduct
SET ClientCode = cd.ClientCode
FROM dbo.ClientPurchasedProduct cpp
INNER JOIN dbo.ClientDetails cd ON cd.ID = cpp.ClientDetailsId
WHERE NULLIF(LTRIM(RTRIM(cpp.ClientCode)), '''') IS NULL;

UPDATE dbo.ClientPurchasedProduct SET ProductCode = '''' WHERE ProductCode IS NULL;
UPDATE dbo.ClientPurchasedProduct SET ProductName = '''' WHERE ProductName IS NULL;
UPDATE dbo.ClientPurchasedProduct SET PricingModel = '''' WHERE PricingModel IS NULL;
UPDATE dbo.ClientPurchasedProduct SET BillingModel = ''One Time'' WHERE NULLIF(LTRIM(RTRIM(BillingModel)), '''') IS NULL;
UPDATE dbo.ClientPurchasedProduct SET BillingFrequency = '''' WHERE BillingFrequency IS NULL;
UPDATE dbo.ClientPurchasedProduct SET BasePrice = 0 WHERE BasePrice IS NULL;
UPDATE dbo.ClientPurchasedProduct SET AmcCalculationType = ''Percentage'' WHERE NULLIF(LTRIM(RTRIM(AmcCalculationType)), '''') IS NULL;
UPDATE dbo.ClientPurchasedProduct SET AmcPercentage = 0 WHERE AmcPercentage IS NULL;
UPDATE dbo.ClientPurchasedProduct SET AmcAmount = 0 WHERE AmcAmount IS NULL;

ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN ClientCode VARCHAR(20) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN ProductCode NVARCHAR(50) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN ProductName NVARCHAR(150) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN PricingModel NVARCHAR(50) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN BillingModel NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN BillingFrequency NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN BasePrice DECIMAL(18,2) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN AmcCalculationType NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN AmcPercentage DECIMAL(18,4) NOT NULL;
ALTER TABLE dbo.ClientPurchasedProduct ALTER COLUMN AmcAmount DECIMAL(18,2) NOT NULL;
');

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ClientPurchasedProduct_ClientDetails')
BEGIN
    EXEC(N'
    ALTER TABLE dbo.ClientPurchasedProduct WITH CHECK ADD CONSTRAINT FK_ClientPurchasedProduct_ClientDetails
    FOREIGN KEY (ClientDetailsId) REFERENCES dbo.ClientDetails(ID) ON DELETE CASCADE;
    ');
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ClientPurchasedProduct_ProductMaster')
BEGIN
    EXEC(N'
    ALTER TABLE dbo.ClientPurchasedProduct WITH CHECK ADD CONSTRAINT FK_ClientPurchasedProduct_ProductMaster
    FOREIGN KEY (ProductId) REFERENCES dbo.ProductMaster(Id);
    ');
END;

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ClientPurchasedProduct_ProductRate')
BEGIN
    EXEC(N'
    ALTER TABLE dbo.ClientPurchasedProduct WITH CHECK ADD CONSTRAINT FK_ClientPurchasedProduct_ProductRate
    FOREIGN KEY (ProductRateId) REFERENCES dbo.ProductRate(Id);
    ');
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ClientPurchasedProduct_ClientCode' AND object_id = OBJECT_ID('dbo.ClientPurchasedProduct'))
BEGIN
    CREATE INDEX IX_ClientPurchasedProduct_ClientCode ON dbo.ClientPurchasedProduct(ClientCode, ProductName, PricingModel);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_ClientPurchasedProduct_ClientDetailsId_ProductRateId' AND object_id = OBJECT_ID('dbo.ClientPurchasedProduct'))
BEGIN
    CREATE UNIQUE INDEX UX_ClientPurchasedProduct_ClientDetailsId_ProductRateId ON dbo.ClientPurchasedProduct(ClientDetailsId, ProductRateId);
END;
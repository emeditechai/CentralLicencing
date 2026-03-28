IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductRateDiscountOffer')
BEGIN
    CREATE TABLE dbo.ProductRateDiscountOffer
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ProductRateId INT NOT NULL,
        DiscountName NVARCHAR(100) NOT NULL,
        DiscountType NVARCHAR(30) NOT NULL,
        DiscountValue DECIMAL(18,2) NOT NULL,
        PromoCode NVARCHAR(50) NULL,
        ValidFrom DATE NOT NULL,
        ValidTo DATE NOT NULL,
        Description NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ProductRateDiscountOffer_IsActive DEFAULT (1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ProductRateDiscountOffer_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ProductRateDiscountOffer_ProductRate'
)
BEGIN
    ALTER TABLE dbo.ProductRateDiscountOffer
    WITH CHECK ADD CONSTRAINT FK_ProductRateDiscountOffer_ProductRate
    FOREIGN KEY (ProductRateId) REFERENCES dbo.ProductRate(Id);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ProductRateDiscountOffer_ProductRateId'
      AND object_id = OBJECT_ID('dbo.ProductRateDiscountOffer')
)
BEGIN
    CREATE INDEX IX_ProductRateDiscountOffer_ProductRateId
        ON dbo.ProductRateDiscountOffer (ProductRateId, ValidFrom DESC, ValidTo DESC);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_ProductRateDiscountOffer_PromoCode'
      AND object_id = OBJECT_ID('dbo.ProductRateDiscountOffer')
)
BEGIN
    CREATE UNIQUE INDEX UX_ProductRateDiscountOffer_PromoCode
        ON dbo.ProductRateDiscountOffer (PromoCode)
        WHERE PromoCode IS NOT NULL;
END;
-- ============================================================
-- 046 : Create TermsConditionTemplate master table
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.tables WHERE name = N'TermsConditionTemplate'
)
BEGIN
    CREATE TABLE dbo.TermsConditionTemplate
    (
        Id          INT             NOT NULL IDENTITY(1,1) PRIMARY KEY,
        TermsName   NVARCHAR(200)   NOT NULL,
        Description NVARCHAR(MAX)   NULL,
        IsActive    BIT             NOT NULL DEFAULT(1),
        CreatedAt   DATETIME        NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT UQ_TermsConditionTemplate_TermsName UNIQUE (TermsName)
    );
END
GO

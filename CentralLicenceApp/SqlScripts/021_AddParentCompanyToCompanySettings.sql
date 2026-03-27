IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CompanySettings')
      AND name = 'ParentCompanyId'
)
BEGIN
    ALTER TABLE dbo.CompanySettings
    ADD ParentCompanyId INT NULL;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_CompanySettings_ParentCompany'
)
BEGIN
    ALTER TABLE dbo.CompanySettings
    WITH CHECK ADD CONSTRAINT FK_CompanySettings_ParentCompany
    FOREIGN KEY (ParentCompanyId) REFERENCES dbo.CompanySettings(Id);
END;
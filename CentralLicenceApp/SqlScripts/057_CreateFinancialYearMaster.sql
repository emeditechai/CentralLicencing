IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FinancialYearMaster')
BEGIN
    CREATE TABLE dbo.FinancialYearMaster (
        Id          INT           IDENTITY(1,1) NOT NULL,
        StartDate   DATE          NOT NULL,
        EndDate     DATE          NOT NULL,
        FYCode      NVARCHAR(10)  NOT NULL,
        IsActive    BIT           NOT NULL DEFAULT(1),
        IsCurrentFY BIT           NOT NULL DEFAULT(0),
        CreatedAt   DATETIME2     NOT NULL DEFAULT(GETDATE()),
        UpdatedAt   DATETIME2     NOT NULL DEFAULT(GETDATE()),
        CONSTRAINT PK_FinancialYearMaster PRIMARY KEY (Id),
        CONSTRAINT UQ_FinancialYearMaster_FYCode UNIQUE (FYCode),
        CONSTRAINT CK_FinancialYearMaster_DateRange CHECK (EndDate > StartDate)
    );

    CREATE NONCLUSTERED INDEX IX_FinancialYearMaster_IsActive
        ON dbo.FinancialYearMaster (IsActive) INCLUDE (StartDate, EndDate, FYCode);
END

-- Add IsCurrentFY column if table already exists without it
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'FinancialYearMaster')
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('FinancialYearMaster') AND name = 'IsCurrentFY')
BEGIN
    ALTER TABLE dbo.FinancialYearMaster ADD IsCurrentFY BIT NOT NULL DEFAULT(0);
END

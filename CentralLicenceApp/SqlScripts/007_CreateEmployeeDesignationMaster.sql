IF OBJECT_ID('dbo.EmployeeDesignationMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeDesignationMaster
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DesignationName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_EmployeeDesignationMaster_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EmployeeDesignationMaster_CreatedAt DEFAULT(GETDATE())
    );

    CREATE UNIQUE INDEX UX_EmployeeDesignationMaster_DesignationName
        ON dbo.EmployeeDesignationMaster(DesignationName);
END
IF OBJECT_ID('dbo.EmployeeDepartmentMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeDepartmentMaster
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DepartmentName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_EmployeeDepartmentMaster_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EmployeeDepartmentMaster_CreatedAt DEFAULT(GETDATE())
    );

    CREATE UNIQUE INDEX UX_EmployeeDepartmentMaster_DepartmentName
        ON dbo.EmployeeDepartmentMaster(DepartmentName);
END
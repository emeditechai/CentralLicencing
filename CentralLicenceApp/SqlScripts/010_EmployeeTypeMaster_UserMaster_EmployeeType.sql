-- =============================================
-- Migration 010: Create EmployeeTypeMaster and add EmployeeTypeId to UserMaster
-- Run on: Central_Lic_DB
-- =============================================

IF OBJECT_ID('dbo.EmployeeTypeMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeTypeMaster
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TypeName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_EmployeeTypeMaster_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EmployeeTypeMaster_CreatedAt DEFAULT(GETDATE())
    );

    CREATE UNIQUE INDEX UX_EmployeeTypeMaster_TypeName
        ON dbo.EmployeeTypeMaster(TypeName);
END

IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypeMaster WHERE TypeName = 'Permanent')
    INSERT INTO dbo.EmployeeTypeMaster (TypeName, Description, IsActive) VALUES ('Permanent', 'Permanent employees', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypeMaster WHERE TypeName = 'Temporary')
    INSERT INTO dbo.EmployeeTypeMaster (TypeName, Description, IsActive) VALUES ('Temporary', 'Temporary employees', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypeMaster WHERE TypeName = 'Outsource')
    INSERT INTO dbo.EmployeeTypeMaster (TypeName, Description, IsActive) VALUES ('Outsource', 'Outsourced staff', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.EmployeeTypeMaster WHERE TypeName = 'Free lancer')
    INSERT INTO dbo.EmployeeTypeMaster (TypeName, Description, IsActive) VALUES ('Free lancer', 'Freelance staff', 1);

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'UserMaster' AND COLUMN_NAME = 'EmployeeTypeId'
)
BEGIN
    ALTER TABLE dbo.UserMaster ADD EmployeeTypeId INT NULL;
    PRINT 'Added EmployeeTypeId column';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_UserMaster_EmployeeTypeMaster_EmployeeTypeId'
)
BEGIN
    ALTER TABLE dbo.UserMaster WITH CHECK
    ADD CONSTRAINT FK_UserMaster_EmployeeTypeMaster_EmployeeTypeId
    FOREIGN KEY (EmployeeTypeId) REFERENCES dbo.EmployeeTypeMaster(Id);
END
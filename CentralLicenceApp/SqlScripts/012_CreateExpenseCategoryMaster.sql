IF OBJECT_ID('dbo.ExpenseCategoryMaster', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ExpenseCategoryMaster
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(200) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_ExpenseCategoryMaster_IsActive DEFAULT(1),
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ExpenseCategoryMaster_CreatedAt DEFAULT(GETDATE())
    );

    CREATE UNIQUE INDEX UX_ExpenseCategoryMaster_CategoryName
        ON dbo.ExpenseCategoryMaster(CategoryName);

    INSERT INTO dbo.ExpenseCategoryMaster (CategoryName, Description, IsActive)
    VALUES
        ('Travel', 'Outstation and intercity travel expenses', 1),
        ('Hotel', 'Accommodation and stay expenses', 1),
        ('Local Travel', 'Taxi, cab, metro, and local conveyance expenses', 1),
        ('Meals', 'Food and refreshment expenses during work travel', 1),
        ('Office Supplies', 'Stationery and office purchase expenses', 1);
END
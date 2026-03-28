IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'tbl_centralemaillog')
BEGIN
    CREATE TABLE dbo.tbl_centralemaillog
    (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EmailType NVARCHAR(150) NOT NULL,
        TemplateKey NVARCHAR(100) NULL,
        RecipientEmail NVARCHAR(200) NULL,
        RecipientName NVARCHAR(200) NULL,
        Subject NVARCHAR(300) NULL,
        Body NVARCHAR(MAX) NULL,
        Status NVARCHAR(30) NOT NULL,
        ErrorMessage NVARCHAR(1000) NULL,
        TriggeredBy NVARCHAR(100) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_tbl_centralemaillog_CreatedAt DEFAULT (GETDATE())
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_tbl_centralemaillog_CreatedAt_EmailType'
      AND object_id = OBJECT_ID('dbo.tbl_centralemaillog')
)
BEGIN
    CREATE INDEX IX_tbl_centralemaillog_CreatedAt_EmailType
        ON dbo.tbl_centralemaillog (CreatedAt DESC, EmailType ASC);
END;
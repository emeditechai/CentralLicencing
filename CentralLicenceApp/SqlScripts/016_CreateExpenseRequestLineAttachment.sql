IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ExpenseRequestLineAttachment')
BEGIN
    CREATE TABLE [dbo].[ExpenseRequestLineAttachment] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [RequestLineId] INT NOT NULL REFERENCES [dbo].[ExpenseRequestLine]([Id]) ON DELETE CASCADE,
        [FilePath] NVARCHAR(300) NOT NULL,
        [OriginalFileName] NVARCHAR(260) NULL,
        [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExpenseRequestLineAttachment_RequestLineId' AND object_id = OBJECT_ID('ExpenseRequestLineAttachment'))
BEGIN
    CREATE INDEX IX_ExpenseRequestLineAttachment_RequestLineId
    ON dbo.ExpenseRequestLineAttachment(RequestLineId, CreatedAt DESC);
END
GO

INSERT INTO ExpenseRequestLineAttachment (RequestLineId, FilePath, OriginalFileName, CreatedAt)
SELECT l.Id,
       l.ReceiptPath,
       RIGHT(l.ReceiptPath, CHARINDEX('/', REVERSE(l.ReceiptPath) + '/') - 1),
       ISNULL(l.CreatedAt, GETDATE())
FROM ExpenseRequestLine l
WHERE l.ReceiptPath IS NOT NULL
  AND LTRIM(RTRIM(l.ReceiptPath)) <> ''
  AND NOT EXISTS (
      SELECT 1
      FROM ExpenseRequestLineAttachment a
      WHERE a.RequestLineId = l.Id
        AND a.FilePath = l.ReceiptPath
  );
GO
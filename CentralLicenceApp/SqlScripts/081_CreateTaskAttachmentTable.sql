-- 081_CreateTaskAttachmentTable.sql
-- Stores file attachment metadata for DailyTaskLog tasks.

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TaskAttachment')
BEGIN
    CREATE TABLE TaskAttachment (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        TaskId        INT NOT NULL,
        FileName      NVARCHAR(260) NOT NULL,   -- stored file name (GUID-based)
        OriginalName  NVARCHAR(260) NOT NULL,   -- original user-supplied file name
        FilePath      NVARCHAR(500) NOT NULL,   -- relative web path (e.g. /uploads/task-attachments/5/abc.pdf)
        FileSize      BIGINT NOT NULL DEFAULT 0,
        UploadedById  INT NOT NULL,
        CreatedAt     DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TaskAttachment_DailyTaskLog FOREIGN KEY (TaskId)
            REFERENCES DailyTaskLog(Id) ON DELETE CASCADE
    );
END

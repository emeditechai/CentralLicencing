-- ============================================================
-- 082 – Task Comments Module
-- ============================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskComment')
BEGIN
    CREATE TABLE TaskComment (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        TaskId      INT             NOT NULL,
        UserId      INT             NOT NULL,
        CommentText NVARCHAR(MAX)   NOT NULL,
        CreatedAt   DATETIME        NOT NULL DEFAULT GETDATE(),

        CONSTRAINT FK_TaskComment_Task FOREIGN KEY (TaskId) REFERENCES DailyTaskLog(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskComment_User FOREIGN KEY (UserId) REFERENCES UserMaster(Id)
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TaskComment_TaskId')
    CREATE INDEX IX_TaskComment_TaskId ON TaskComment(TaskId);

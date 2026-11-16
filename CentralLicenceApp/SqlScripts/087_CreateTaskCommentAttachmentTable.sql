-- ============================================================
-- 087 – Attachments on task comments
-- Idempotent: safe to run more than once.
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TaskCommentAttachment')
BEGIN
    CREATE TABLE TaskCommentAttachment (
        Id            INT IDENTITY(1,1) PRIMARY KEY,
        CommentId     INT NOT NULL,
        TaskId        INT NOT NULL,
        OriginalName  NVARCHAR(260) NOT NULL,
        StoredName    NVARCHAR(100) NOT NULL,   -- GUID-based name on disk
        FileSize      BIGINT NOT NULL DEFAULT 0,
        UploadedById  INT NOT NULL,
        CreatedAt     DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_TaskCommentAttachment_Comment FOREIGN KEY (CommentId) REFERENCES TaskComment(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskCommentAttachment_User FOREIGN KEY (UploadedById) REFERENCES UserMaster(Id)
    );
    CREATE INDEX IX_TaskCommentAttachment_Comment ON TaskCommentAttachment(CommentId);
END
GO

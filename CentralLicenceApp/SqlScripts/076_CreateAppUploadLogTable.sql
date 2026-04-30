-- 076: Create AppUploadLog table for APK/iOS file uploads

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUploadLog')
BEGIN
    CREATE TABLE [dbo].[AppUploadLog] (
        [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Platform]     NVARCHAR(10)   NOT NULL,          -- 'Android' or 'iOS'
        [FileName]     NVARCHAR(260)  NOT NULL,
        [OriginalName] NVARCHAR(260)  NOT NULL,
        [FileSizeBytes] BIGINT        NOT NULL DEFAULT 0,
        [DownloadUrl]  NVARCHAR(500)  NOT NULL,
        [UploadedBy]   NVARCHAR(100)  NOT NULL,
        [UploadedAt]   DATETIME       NOT NULL DEFAULT GETDATE(),
        [Notes]        NVARCHAR(500)  NULL
    );
END

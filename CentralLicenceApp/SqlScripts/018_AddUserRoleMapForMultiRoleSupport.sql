IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoleMap')
BEGIN
    CREATE TABLE [dbo].[UserRoleMap] (
        [UserId] INT NOT NULL,
        [RoleId] INT NOT NULL,
        [CreatedAt] DATETIME NOT NULL CONSTRAINT [DF_UserRoleMap_CreatedAt] DEFAULT GETDATE(),
        CONSTRAINT [PK_UserRoleMap] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoleMap_UserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserMaster]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoleMap_RoleMaster] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[RoleMaster]([Id])
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_UserRoleMap_RoleId'
      AND object_id = OBJECT_ID('dbo.UserRoleMap'))
BEGIN
    CREATE INDEX [IX_UserRoleMap_RoleId] ON [dbo].[UserRoleMap]([RoleId], [UserId]);
END
GO

INSERT INTO [dbo].[UserRoleMap] ([UserId], [RoleId], [CreatedAt])
SELECT u.Id, u.RoleId, GETDATE()
FROM [dbo].[UserMaster] u
WHERE u.RoleId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1
      FROM [dbo].[UserRoleMap] ur
      WHERE ur.UserId = u.Id
        AND ur.RoleId = u.RoleId
  );
GO

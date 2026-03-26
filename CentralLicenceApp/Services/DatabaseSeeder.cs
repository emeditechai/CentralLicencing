using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Services
{
    public class DatabaseSeeder
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(IConfiguration config, ILogger<DatabaseSeeder> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")!;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);

                await EnsureRoleMasterAsync(conn);
                await EnsureLocationMasterAsync(conn);
                await EnsureEmployeeDepartmentMasterAsync(conn);
                await EnsureEmployeeDesignationMasterAsync(conn);
                await EnsureEmployeeTypeMasterAsync(conn);
                await EnsureExpenseCategoryMasterAsync(conn);
                await EnsureExpenseRequestTablesAsync(conn);
                await EnsureUserPushSubscriptionTableAsync(conn);
                await EnsureUserMasterAsync(conn);
                await SeedDefaultUsersAsync(conn);
                await EnsureCompanySettingsTablesAsync(conn);
                await EnsureEmailTemplatesTableAsync(conn);
                await EnsureEmailRemindersTableAsync(conn);
                await EnsureClientDetailsTableAsync(conn);
                await SeedDefaultEmailTemplatesAsync(conn);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database seeding failed.");
            }
        }

        private static async Task EnsureRoleMasterAsync(SqlConnection conn)
        {
            var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='RoleMaster')
                BEGIN
                    CREATE TABLE [dbo].[RoleMaster] (
                        [Id]          INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [RoleName]    NVARCHAR(50)  NOT NULL,
                        [Description] NVARCHAR(200) NULL,
                        [IsActive]    BIT           NOT NULL DEFAULT 1,
                        [CreatedAt]   DATETIME      NOT NULL DEFAULT GETDATE()
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName='Administrator')
                    INSERT INTO RoleMaster(RoleName,Description,IsActive) VALUES('Administrator','Full access',1);

                IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName='Staff')
                  INSERT INTO RoleMaster(RoleName,Description,IsActive) VALUES('Staff','Read-only access',1);

                IF NOT EXISTS (SELECT 1 FROM RoleMaster WHERE RoleName='Finance')
                  INSERT INTO RoleMaster(RoleName,Description,IsActive) VALUES('Finance','Expense reimbursement and settlement access',1);";
            await conn.ExecuteAsync(sql);
        }

        private static async Task EnsureUserMasterAsync(SqlConnection conn)
        {
            var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='UserMaster')
                BEGIN
                    CREATE TABLE [dbo].[UserMaster] (
                        [Id]            INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [Username]      NVARCHAR(100)  NOT NULL,
                        [Email]         NVARCHAR(200)  NOT NULL,
                        [PasswordHash]  NVARCHAR(500)  NOT NULL,
                        [FullName]      NVARCHAR(200)  NULL,
                    [PhoneNumber]   NVARCHAR(20)   NULL,
                    [DateOfBirth]   DATE           NULL,
                    [DateOfJoining] DATE           NULL,
                        [RoleId]        INT            NOT NULL REFERENCES [dbo].[RoleMaster]([Id]),
                [LocationId]    INT            NULL REFERENCES [dbo].[LocationMaster]([Id]),
                [DepartmentId]  INT            NULL REFERENCES [dbo].[EmployeeDepartmentMaster]([Id]),
                [DesignationId] INT            NULL REFERENCES [dbo].[EmployeeDesignationMaster]([Id]),
                [EmployeeTypeId] INT           NULL REFERENCES [dbo].[EmployeeTypeMaster]([Id]),
                [IsEmployee]    BIT            NOT NULL DEFAULT 0,
                [EmployeeCode]  NVARCHAR(50)   NULL,
                [IsCoreMember]  BIT            NOT NULL DEFAULT 0,
                [ManagerId]     INT            NULL REFERENCES [dbo].[UserMaster]([Id]),
                [ProfileImagePath] NVARCHAR(300) NULL,
                        [IsActive]      BIT            NOT NULL DEFAULT 1,
                        [CreatedAt]     DATETIME       NOT NULL DEFAULT GETDATE(),
                        [LastLoginDate] DATETIME       NULL
                    );
            END
            ELSE
            BEGIN
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'PhoneNumber')
                ALTER TABLE [dbo].[UserMaster] ADD [PhoneNumber] NVARCHAR(20) NULL;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DateOfBirth')
                ALTER TABLE [dbo].[UserMaster] ADD [DateOfBirth] DATE NULL;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DateOfJoining')
                ALTER TABLE [dbo].[UserMaster] ADD [DateOfJoining] DATE NULL;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'LocationId')
                ALTER TABLE [dbo].[UserMaster] ADD [LocationId] INT NULL REFERENCES [dbo].[LocationMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DepartmentId')
                ALTER TABLE [dbo].[UserMaster] ADD [DepartmentId] INT NULL REFERENCES [dbo].[EmployeeDepartmentMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'DesignationId')
                ALTER TABLE [dbo].[UserMaster] ADD [DesignationId] INT NULL REFERENCES [dbo].[EmployeeDesignationMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'EmployeeTypeId')
                ALTER TABLE [dbo].[UserMaster] ADD [EmployeeTypeId] INT NULL REFERENCES [dbo].[EmployeeTypeMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'IsEmployee')
                ALTER TABLE [dbo].[UserMaster] ADD [IsEmployee] BIT NOT NULL DEFAULT 0;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'EmployeeCode')
                ALTER TABLE [dbo].[UserMaster] ADD [EmployeeCode] NVARCHAR(50) NULL;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'IsCoreMember')
                ALTER TABLE [dbo].[UserMaster] ADD [IsCoreMember] BIT NOT NULL DEFAULT 0;
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'ManagerId')
                ALTER TABLE [dbo].[UserMaster] ADD [ManagerId] INT NULL REFERENCES [dbo].[UserMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('UserMaster') AND name = 'ProfileImagePath')
                ALTER TABLE [dbo].[UserMaster] ADD [ProfileImagePath] NVARCHAR(300) NULL;
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='UserRoleMap')
            BEGIN
                CREATE TABLE [dbo].[UserRoleMap] (
                    [UserId]    INT       NOT NULL,
                    [RoleId]    INT       NOT NULL,
                    [CreatedAt] DATETIME  NOT NULL DEFAULT GETDATE(),
                    CONSTRAINT [PK_UserRoleMap] PRIMARY KEY ([UserId], [RoleId]),
                    CONSTRAINT [FK_UserRoleMap_UserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[UserMaster]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_UserRoleMap_RoleMaster] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[RoleMaster]([Id])
                );
            END

            IF NOT EXISTS (
              SELECT 1 FROM sys.indexes
              WHERE name = 'IX_UserRoleMap_RoleId'
                AND object_id = OBJECT_ID('UserRoleMap'))
            BEGIN
              CREATE INDEX [IX_UserRoleMap_RoleId] ON [dbo].[UserRoleMap]([RoleId], [UserId]);
            END

            INSERT INTO [dbo].[UserRoleMap]([UserId], [RoleId], [CreatedAt])
            SELECT u.Id, u.RoleId, GETDATE()
            FROM [dbo].[UserMaster] u
            WHERE u.RoleId IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM [dbo].[UserRoleMap] ur
                  WHERE ur.UserId = u.Id AND ur.RoleId = u.RoleId
              );

            IF NOT EXISTS (
              SELECT 1 FROM sys.indexes
              WHERE name = 'UQ_UserMaster_EmployeeCode'
                AND object_id = OBJECT_ID('UserMaster'))
            BEGIN
              CREATE UNIQUE INDEX [UQ_UserMaster_EmployeeCode]
                ON [dbo].[UserMaster] ([EmployeeCode])
                WHERE [EmployeeCode] IS NOT NULL;
            END";
            await conn.ExecuteAsync(sql);
        }

          private static async Task EnsureLocationMasterAsync(SqlConnection conn)
          {
            await conn.ExecuteAsync(@"
              IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='LocationMaster')
              BEGIN
                CREATE TABLE [dbo].[LocationMaster] (
                  [Id]        INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [Name]      NVARCHAR(100) NOT NULL,
                  [IsActive]  BIT           NOT NULL DEFAULT 1,
                  [CreatedAt] DATETIME      NOT NULL DEFAULT GETDATE()
                );
              END

              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Kolkata')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Kolkata', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Mumbai')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Mumbai', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Delhi')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Delhi', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Bengaluru')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Bengaluru', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Chennai')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Chennai', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Hyderabad')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Hyderabad', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Pune')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Pune', 1, GETDATE());
              IF NOT EXISTS (SELECT 1 FROM LocationMaster WHERE Name = 'Ahmedabad')
                INSERT INTO LocationMaster (Name, IsActive, CreatedAt) VALUES ('Ahmedabad', 1, GETDATE());
            ");
          }

          private static async Task EnsureEmployeeDepartmentMasterAsync(SqlConnection conn)
          {
            await conn.ExecuteAsync(@"
              IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='EmployeeDepartmentMaster')
              BEGIN
                CREATE TABLE [dbo].[EmployeeDepartmentMaster] (
                  [Id]             INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [DepartmentName] NVARCHAR(100)  NOT NULL,
                  [Description]    NVARCHAR(200)  NULL,
                  [IsActive]       BIT            NOT NULL DEFAULT 1,
                  [CreatedAt]      DATETIME       NOT NULL DEFAULT GETDATE()
                );
              END

              IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = 'UX_EmployeeDepartmentMaster_DepartmentName'
                  AND object_id = OBJECT_ID('EmployeeDepartmentMaster'))
              BEGIN
                CREATE UNIQUE INDEX UX_EmployeeDepartmentMaster_DepartmentName
                  ON dbo.EmployeeDepartmentMaster(DepartmentName);
              END
            ");
          }

          private static async Task EnsureEmployeeDesignationMasterAsync(SqlConnection conn)
          {
            await conn.ExecuteAsync(@"
              IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='EmployeeDesignationMaster')
              BEGIN
                CREATE TABLE [dbo].[EmployeeDesignationMaster] (
                  [Id]              INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                  [DesignationName] NVARCHAR(100)  NOT NULL,
                  [Description]     NVARCHAR(200)  NULL,
                  [IsActive]        BIT            NOT NULL DEFAULT 1,
                  [CreatedAt]       DATETIME       NOT NULL DEFAULT GETDATE()
                );
              END

              IF NOT EXISTS (
                SELECT 1 FROM sys.indexes
                WHERE name = 'UX_EmployeeDesignationMaster_DesignationName'
                  AND object_id = OBJECT_ID('EmployeeDesignationMaster'))
              BEGIN
                CREATE UNIQUE INDEX UX_EmployeeDesignationMaster_DesignationName
                  ON dbo.EmployeeDesignationMaster(DesignationName);
              END
            ");
          }

        private static async Task EnsureEmployeeTypeMasterAsync(SqlConnection conn)
        {
          await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='EmployeeTypeMaster')
            BEGIN
              CREATE TABLE [dbo].[EmployeeTypeMaster] (
                [Id]         INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [TypeName]   NVARCHAR(100)  NOT NULL,
                [Description] NVARCHAR(200) NULL,
                [IsActive]   BIT            NOT NULL DEFAULT 1,
                [CreatedAt]  DATETIME       NOT NULL DEFAULT GETDATE(),
                CONSTRAINT [UQ_EmployeeTypeMaster_TypeName] UNIQUE ([TypeName])
              );
            END

            IF NOT EXISTS (SELECT 1 FROM EmployeeTypeMaster WHERE TypeName = 'Permanent')
              INSERT INTO EmployeeTypeMaster (TypeName, Description, IsActive, CreatedAt)
              VALUES ('Permanent', 'Permanent employees', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM EmployeeTypeMaster WHERE TypeName = 'Temporary')
              INSERT INTO EmployeeTypeMaster (TypeName, Description, IsActive, CreatedAt)
              VALUES ('Temporary', 'Temporary employees', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM EmployeeTypeMaster WHERE TypeName = 'Outsource')
              INSERT INTO EmployeeTypeMaster (TypeName, Description, IsActive, CreatedAt)
              VALUES ('Outsource', 'Outsourced staff', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM EmployeeTypeMaster WHERE TypeName = 'Free lancer')
              INSERT INTO EmployeeTypeMaster (TypeName, Description, IsActive, CreatedAt)
              VALUES ('Free lancer', 'Freelance staff', 1, GETDATE());
          ");
        }

        private static async Task EnsureExpenseCategoryMasterAsync(SqlConnection conn)
        {
          await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ExpenseCategoryMaster')
            BEGIN
              CREATE TABLE [dbo].[ExpenseCategoryMaster] (
                [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [CategoryName] NVARCHAR(100)  NOT NULL,
                [Description]  NVARCHAR(200)  NULL,
                [IsActive]     BIT            NOT NULL DEFAULT 1,
                [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE()
              );
            END

            IF NOT EXISTS (
              SELECT 1 FROM sys.indexes
              WHERE name = 'UX_ExpenseCategoryMaster_CategoryName'
                AND object_id = OBJECT_ID('ExpenseCategoryMaster'))
            BEGIN
              CREATE UNIQUE INDEX UX_ExpenseCategoryMaster_CategoryName
                ON dbo.ExpenseCategoryMaster(CategoryName);
            END

            IF NOT EXISTS (SELECT 1 FROM ExpenseCategoryMaster WHERE CategoryName = 'Travel')
              INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
              VALUES ('Travel', 'Outstation and intercity travel expenses', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM ExpenseCategoryMaster WHERE CategoryName = 'Hotel')
              INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
              VALUES ('Hotel', 'Accommodation and stay expenses', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM ExpenseCategoryMaster WHERE CategoryName = 'Local Travel')
              INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
              VALUES ('Local Travel', 'Taxi, cab, metro, and local conveyance expenses', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM ExpenseCategoryMaster WHERE CategoryName = 'Meals')
              INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
              VALUES ('Meals', 'Food and refreshment expenses during work travel', 1, GETDATE());

            IF NOT EXISTS (SELECT 1 FROM ExpenseCategoryMaster WHERE CategoryName = 'Office Supplies')
              INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
              VALUES ('Office Supplies', 'Stationery and office purchase expenses', 1, GETDATE());
          ");
        }

        private static async Task EnsureExpenseRequestTablesAsync(SqlConnection conn)
        {
          await conn.ExecuteAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ExpenseRequest')
            BEGIN
              CREATE TABLE [dbo].[ExpenseRequest] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [RequestNumber] NVARCHAR(30) NOT NULL,
                [EmployeeId] INT NOT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [ApproverId] INT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [PurposeOfTravel] NVARCHAR(200) NOT NULL,
                [EmployeeRemarks] NVARCHAR(500) NULL,
                [Status] NVARCHAR(30) NOT NULL,
                [TotalAmount] DECIMAL(18,2) NOT NULL DEFAULT 0,
                [ItemCount] INT NOT NULL DEFAULT 0,
                [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE(),
                [SubmittedAt] DATETIME NULL,
                [ApprovedAt] DATETIME NULL,
                [RejectedAt] DATETIME NULL,
                [ApprovalRemarks] NVARCHAR(500) NULL,
                [ApprovedById] INT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [ReimbursementStartedAt] DATETIME NULL,
                [ReimbursementStartedById] INT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [ReimbursementRemarks] NVARCHAR(500) NULL,
                [SettlementAmount] DECIMAL(18,2) NULL,
                [SettlementDate] DATE NULL,
                [SettledAt] DATETIME NULL,
                [SettledById] INT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [SettlementMode] NVARCHAR(30) NULL,
                [SettlementReferenceNo] NVARCHAR(100) NULL,
                [SettlementRemarks] NVARCHAR(500) NULL,
                [SettlementReceiptNumber] NVARCHAR(40) NULL
              );
            END
            ELSE
            BEGIN
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'ReimbursementStartedAt')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [ReimbursementStartedAt] DATETIME NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'ReimbursementStartedById')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [ReimbursementStartedById] INT NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'ReimbursementRemarks')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [ReimbursementRemarks] NVARCHAR(500) NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementAmount')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementAmount] DECIMAL(18,2) NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementDate')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementDate] DATE NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettledAt')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettledAt] DATETIME NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettledById')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettledById] INT NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementMode')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementMode] NVARCHAR(30) NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementReferenceNo')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementReferenceNo] NVARCHAR(100) NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementRemarks')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementRemarks] NVARCHAR(500) NULL;
              IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ExpenseRequest') AND name = 'SettlementReceiptNumber')
                ALTER TABLE [dbo].[ExpenseRequest] ADD [SettlementReceiptNumber] NVARCHAR(40) NULL;

              IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExpenseRequest_ReimbursementStartedBy')
                ALTER TABLE [dbo].[ExpenseRequest] ADD CONSTRAINT [FK_ExpenseRequest_ReimbursementStartedBy] FOREIGN KEY ([ReimbursementStartedById]) REFERENCES [dbo].[UserMaster]([Id]);
              IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExpenseRequest_SettledBy')
                ALTER TABLE [dbo].[ExpenseRequest] ADD CONSTRAINT [FK_ExpenseRequest_SettledBy] FOREIGN KEY ([SettledById]) REFERENCES [dbo].[UserMaster]([Id]);
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ExpenseRequestLine')
            BEGIN
              CREATE TABLE [dbo].[ExpenseRequestLine] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [RequestId] INT NOT NULL REFERENCES [dbo].[ExpenseRequest]([Id]) ON DELETE CASCADE,
                [ItemType] NVARCHAR(30) NOT NULL,
                [ExpenseCategoryId] INT NULL REFERENCES [dbo].[ExpenseCategoryMaster]([Id]),
                [Title] NVARCHAR(150) NOT NULL,
                [ProjectOrCostCenter] NVARCHAR(120) NULL,
                [ExpenseDate] DATE NOT NULL,
                [CurrencyCode] NVARCHAR(10) NOT NULL,
                [Amount] DECIMAL(18,2) NOT NULL,
                [PayableAmountInr] DECIMAL(18,2) NULL,
                [AccommodationCountry] NVARCHAR(100) NULL,
                [AccommodationCity] NVARCHAR(100) NULL,
                [CheckInDate] DATE NULL,
                [CheckOutDate] DATE NULL,
                [ReceiptPath] NVARCHAR(300) NULL,
                [Notes] NVARCHAR(500) NULL,
                [CreatedAt] DATETIME NOT NULL DEFAULT GETDATE()
              );
            END

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

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ExpenseRequestApprovalHistory')
            BEGIN
              CREATE TABLE [dbo].[ExpenseRequestApprovalHistory] (
                [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                [RequestId] INT NOT NULL REFERENCES [dbo].[ExpenseRequest]([Id]) ON DELETE CASCADE,
                [ActionTaken] NVARCHAR(50) NOT NULL,
                [ActionByUserId] INT NULL REFERENCES [dbo].[UserMaster]([Id]),
                [Remarks] NVARCHAR(500) NULL,
                [ActionAt] DATETIME NOT NULL DEFAULT GETDATE()
              );
            END

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_ExpenseRequest_RequestNumber' AND object_id = OBJECT_ID('ExpenseRequest'))
              CREATE UNIQUE INDEX UX_ExpenseRequest_RequestNumber ON dbo.ExpenseRequest(RequestNumber);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExpenseRequest_Employee_Status' AND object_id = OBJECT_ID('ExpenseRequest'))
              CREATE INDEX IX_ExpenseRequest_Employee_Status ON dbo.ExpenseRequest(EmployeeId, Status, CreatedAt DESC);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExpenseRequest_Approver_Status' AND object_id = OBJECT_ID('ExpenseRequest'))
              CREATE INDEX IX_ExpenseRequest_Approver_Status ON dbo.ExpenseRequest(ApproverId, Status, SubmittedAt DESC);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExpenseRequestLine_RequestId' AND object_id = OBJECT_ID('ExpenseRequestLine'))
              CREATE INDEX IX_ExpenseRequestLine_RequestId ON dbo.ExpenseRequestLine(RequestId, ExpenseDate DESC);

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExpenseRequestLineAttachment_RequestLineId' AND object_id = OBJECT_ID('ExpenseRequestLineAttachment'))
              CREATE INDEX IX_ExpenseRequestLineAttachment_RequestLineId ON dbo.ExpenseRequestLineAttachment(RequestLineId, CreatedAt DESC);
          ");
        }

        private static async Task SeedDefaultUsersAsync(SqlConnection conn)
        {
            var adminRoleId = await conn.ExecuteScalarAsync<int>(
                "SELECT Id FROM RoleMaster WHERE RoleName='Administrator'");
            var staffRoleId = await conn.ExecuteScalarAsync<int>(
                "SELECT Id FROM RoleMaster WHERE RoleName='Staff'");

            if (!await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM UserMaster WHERE Username='admin'"))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("Admin@1234");
                await conn.ExecuteAsync(@"
                    INSERT INTO UserMaster(Username,Email,PasswordHash,FullName,RoleId,IsActive,CreatedAt)
                    VALUES('admin','admin@centrallicence.com',@Hash,'System Administrator',@RoleId,1,GETDATE())",
                    new { Hash = hash, RoleId = adminRoleId });
            }

                await conn.ExecuteAsync(@"
                  INSERT INTO UserRoleMap(UserId, RoleId, CreatedAt)
                  SELECT u.Id, @RoleId, GETDATE()
                  FROM UserMaster u
                  WHERE u.Username = 'admin'
                    AND NOT EXISTS (
                      SELECT 1 FROM UserRoleMap ur
                      WHERE ur.UserId = u.Id AND ur.RoleId = @RoleId)",
                  new { RoleId = adminRoleId });

            if (!await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM UserMaster WHERE Username='staff'"))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("Staff@1234");
                await conn.ExecuteAsync(@"
                    INSERT INTO UserMaster(Username,Email,PasswordHash,FullName,RoleId,IsActive,CreatedAt)
                    VALUES('staff','staff@centrallicence.com',@Hash,'Staff Member',@RoleId,1,GETDATE())",
                    new { Hash = hash, RoleId = staffRoleId });
            }

                await conn.ExecuteAsync(@"
                  INSERT INTO UserRoleMap(UserId, RoleId, CreatedAt)
                  SELECT u.Id, @RoleId, GETDATE()
                  FROM UserMaster u
                  WHERE u.Username = 'staff'
                    AND NOT EXISTS (
                      SELECT 1 FROM UserRoleMap ur
                      WHERE ur.UserId = u.Id AND ur.RoleId = @RoleId)",
                  new { RoleId = staffRoleId });
        }

        private static async Task EnsureEmailTemplatesTableAsync(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='tbl_centralemailtemplates')
                BEGIN
                    CREATE TABLE [dbo].[tbl_centralemailtemplates] (
                        [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [TemplateKey]  NVARCHAR(100)  NOT NULL,
                        [TemplateName] NVARCHAR(200)  NOT NULL,
                        [Subject]      NVARCHAR(500)  NOT NULL,
                        [Body]         NVARCHAR(MAX)  NOT NULL,
                        [IsActive]     BIT            NOT NULL DEFAULT 1,
                        [CreatedAt]    DATETIME       NOT NULL DEFAULT GETDATE(),
                        [UpdatedAt]    DATETIME       NULL,
                        CONSTRAINT [UQ_TemplateKey] UNIQUE ([TemplateKey])
                    );
                END");
        }

        private static async Task EnsureUserPushSubscriptionTableAsync(SqlConnection conn)
        {
          await conn.ExecuteAsync(@"
            IF OBJECT_ID('dbo.UserPushSubscription', 'U') IS NULL
            BEGIN
              CREATE TABLE dbo.UserPushSubscription
              (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                UserId INT NOT NULL,
                Endpoint NVARCHAR(1000) NOT NULL,
                P256dh NVARCHAR(300) NOT NULL,
                Auth NVARCHAR(200) NOT NULL,
                UserAgent NVARCHAR(500) NULL,
                IsActive BIT NOT NULL CONSTRAINT DF_UserPushSubscription_IsActive DEFAULT(1),
                CreatedAt DATETIME NOT NULL CONSTRAINT DF_UserPushSubscription_CreatedAt DEFAULT(GETDATE()),
                UpdatedAt DATETIME NOT NULL CONSTRAINT DF_UserPushSubscription_UpdatedAt DEFAULT(GETDATE()),
                CONSTRAINT FK_UserPushSubscription_UserMaster FOREIGN KEY (UserId) REFERENCES dbo.UserMaster(Id) ON DELETE CASCADE,
                CONSTRAINT UQ_UserPushSubscription_Endpoint UNIQUE (Endpoint)
              );
            END

            IF NOT EXISTS (
              SELECT 1 FROM sys.indexes
              WHERE name = 'IX_UserPushSubscription_UserId'
                AND object_id = OBJECT_ID('dbo.UserPushSubscription'))
            BEGIN
              CREATE INDEX IX_UserPushSubscription_UserId
              ON dbo.UserPushSubscription(UserId, IsActive, UpdatedAt DESC);
            END");
        }

        private static async Task EnsureEmailRemindersTableAsync(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='tbl_centralemailreminders')
                BEGIN
                    CREATE TABLE [dbo].[tbl_centralemailreminders] (
                        [Id]           INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [LicenseId]    INT            NOT NULL,
                        [ReminderType] NVARCHAR(50)   NOT NULL,
                        [SentAt]       DATETIME       NOT NULL DEFAULT GETDATE(),
                        [ToEmail]      NVARCHAR(200)  NOT NULL
                    );
                END");
        }

        private static async Task EnsureClientDetailsTableAsync(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='ClientDetails')
                BEGIN
                    CREATE TABLE [dbo].[ClientDetails] (
                        [ID]               INT            IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [ClientCode]       VARCHAR(20)    NOT NULL,
                        [ClientPersonName] VARCHAR(100)   NULL,
                        [address]          VARCHAR(100)   NULL,
                        [ProductPurchased] VARCHAR(200)   NULL,
                        [DOB]              DATE           NULL,
                        [Anniversarydate]  DATE           NULL,
                        [IsActive]         BIT            NOT NULL DEFAULT 1,
                        CONSTRAINT [UQ_ClientDetails_ClientCode] UNIQUE ([ClientCode])
                    );
                END
                ELSE
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='ClientPersonName')
                        ALTER TABLE [dbo].[ClientDetails] ADD [ClientPersonName] VARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='address')
                        ALTER TABLE [dbo].[ClientDetails] ADD [address] VARCHAR(100) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='ProductPurchased')
                        ALTER TABLE [dbo].[ClientDetails] ADD [ProductPurchased] VARCHAR(200) NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='DOB')
                        ALTER TABLE [dbo].[ClientDetails] ADD [DOB] DATE NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='Anniversarydate')
                        ALTER TABLE [dbo].[ClientDetails] ADD [Anniversarydate] DATE NULL;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id=OBJECT_ID('ClientDetails') AND name='IsActive')
                        ALTER TABLE [dbo].[ClientDetails] ADD [IsActive] BIT NOT NULL DEFAULT 1;
                END");
        }

        private static async Task EnsureCompanySettingsTablesAsync(SqlConnection conn)
        {
            await conn.ExecuteAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='CompanyTypeMaster')
                    BEGIN
                      CREATE TABLE [dbo].[CompanyTypeMaster] (
                        [Id]         INT           IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [TypeName]   NVARCHAR(100) NOT NULL,
                        [IsActive]   BIT           NOT NULL DEFAULT 1,
                        [CreatedAt]  DATETIME      NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [UQ_CompanyTypeMaster_TypeName] UNIQUE ([TypeName])
                      );
                    END

                    IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'Private')
                      INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('Private', 1, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'LLP')
                      INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('LLP', 1, GETDATE());

                    IF NOT EXISTS (SELECT 1 FROM CompanyTypeMaster WHERE TypeName = 'Services')
                      INSERT INTO CompanyTypeMaster (TypeName, IsActive, CreatedAt) VALUES ('Services', 1, GETDATE());

                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='CompanySettings')
                    BEGIN
                      CREATE TABLE [dbo].[CompanySettings] (
                        [Id]               INT             IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [CompanyCode]      NVARCHAR(50)    NOT NULL,
                        [CompanyTypeId]    INT             NOT NULL REFERENCES [dbo].[CompanyTypeMaster]([Id]),
                        [CompanyName]      NVARCHAR(200)   NOT NULL,
                        [Country]          NVARCHAR(100)   NULL,
                        [State]            NVARCHAR(100)   NULL,
                        [District]         NVARCHAR(100)   NULL,
                        [City]             NVARCHAR(100)   NULL,
                        [Address]          NVARCHAR(500)   NULL,
                        [Website]          NVARCHAR(200)   NULL,
                        [EmailId]          NVARCHAR(200)   NULL,
                        [ContactNo]        NVARCHAR(30)    NULL,
                        [Pincode]          NVARCHAR(20)    NULL,
                        [GSTCode]          NVARCHAR(50)    NULL,
                        [PANCard]          NVARCHAR(50)    NULL,
                        [IsParentCompany]  BIT             NOT NULL DEFAULT 0,
                        [IsExpenseEmailNotificationRequired] BIT NOT NULL DEFAULT 0,
                        [CompanyLogoPath]  NVARCHAR(300)   NULL,
                        [IsActive]         BIT             NOT NULL DEFAULT 1,
                        [CreatedAt]        DATETIME        NOT NULL DEFAULT GETDATE(),
                        CONSTRAINT [UQ_CompanySettings_CompanyCode] UNIQUE ([CompanyCode])
                      );
                    END
                    ELSE
                    BEGIN
                      IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'Website')
                        ALTER TABLE [dbo].[CompanySettings] ADD [Website] NVARCHAR(200) NULL;
                      IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'EmailId')
                        ALTER TABLE [dbo].[CompanySettings] ADD [EmailId] NVARCHAR(200) NULL;
                      IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'ContactNo')
                        ALTER TABLE [dbo].[CompanySettings] ADD [ContactNo] NVARCHAR(30) NULL;
                      IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('CompanySettings') AND name = 'IsExpenseEmailNotificationRequired')
                        ALTER TABLE [dbo].[CompanySettings] ADD [IsExpenseEmailNotificationRequired] BIT NOT NULL CONSTRAINT [DF_CompanySettings_IsExpenseEmailNotificationRequired] DEFAULT 0;
                    END");
        }

        private static async Task SeedDefaultEmailTemplatesAsync(SqlConnection conn)
        {
            const string activatedHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""580"" cellpadding=""0"" cellspacing=""0""
           style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#6366f1 0%,#4f46e5 100%);padding:36px 40px;text-align:center;"">
          <p style=""margin:0;color:#c7d2fe;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:8px 0 4px;color:#fff;font-size:26px;font-weight:700;"">Welcome Back! &#127881;</h1>
          <p style=""margin:0;color:#e0e7ff;font-size:14px;"">Your {{AppName}} licence is now active</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:36px 40px;"">
          <p style=""margin:0 0 16px;color:#374151;font-size:15px;"">Dear <strong>{{ClientName}}</strong>,</p>
          <p style=""margin:0 0 24px;color:#374151;font-size:14px;line-height:1.7;"">
            Great news! Your <strong>{{AppName}}</strong> licence (<strong>{{ClientCode}}</strong>) has been
            <span style=""color:#16a34a;font-weight:600;"">reactivated</span>. You can now access all features of the application.
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                 style=""background:#f5f3ff;border-radius:10px;border:1px solid #ddd6fe;margin-bottom:24px;"">
            <tr><td style=""padding:20px 24px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"">
                <tr>
                  <td style=""color:#6b7280;font-size:13px;width:45%;border-bottom:1px solid #ede9fe;"">Client Code</td>
                  <td style=""color:#1e293b;font-size:13px;font-weight:700;border-bottom:1px solid #ede9fe;"">{{ClientCode}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;border-bottom:1px solid #ede9fe;"">Licence Expiry</td>
                  <td style=""color:#1e293b;font-size:13px;font-weight:700;border-bottom:1px solid #ede9fe;"">{{ExpiryDate}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;"">Application URL</td>
                  <td style=""font-size:13px;""><a href=""{{AppUrl}}"" style=""color:#6366f1;font-weight:600;"">{{AppUrl}}</a></td>
                </tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0;color:#9ca3af;font-size:13px;"">If you need assistance, please contact your support representative.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #f1f5f9;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 {{CompanyName}} &middot; Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string settlementCompletedHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:28px 14px;"">
    <table width=""620"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 28px rgba(15,23,42,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#14532d 0%,#0f766e 100%);padding:28px 34px;"">
          <p style=""margin:0 0 8px;color:#ccfbf1;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:700;"">Expense Settlement Completed</h1>
          <p style=""margin:8px 0 0;color:#d1fae5;font-size:13px;line-height:1.6;"">Your approved request has been processed and settled successfully.</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:30px 34px;"">
          <p style=""margin:0 0 16px;color:#334155;font-size:14px;line-height:1.75;"">Dear <strong>{{EmployeeName}}</strong>, your request <strong>{{RequestNumber}}</strong> has been settled. The payment summary is below.</p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border:1px solid #dbe3ef;border-radius:12px;margin-bottom:20px;"">
            <tr><td style=""padding:18px 20px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"" style=""font-size:13px;color:#334155;"">
                <tr><td style=""width:42%;color:#64748b;border-bottom:1px solid #e2e8f0;"">Receipt No</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{SettlementReceiptNumber}}</td></tr>
                <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Settlement Date</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{SettlementDate}}</td></tr>
                <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Settled Amount</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{SettlementAmount}}</td></tr>
                <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Payment Mode</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{SettlementMode}}</td></tr>
                <tr><td style=""color:#64748b;"">Reference No</td><td style=""font-weight:700;"">{{SettlementReferenceNo}}</td></tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0 0 14px;color:#475569;font-size:13px;line-height:1.7;"">Purpose: <strong>{{PurposeOfTravel}}</strong></p>
          <a href=""{{SettlementReceiptUrl}}"" style=""display:inline-block;background:#14532d;color:#ffffff;text-decoration:none;padding:12px 18px;border-radius:10px;font-size:13px;font-weight:700;"">View Settlement Receipt</a>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:14px 34px;border-top:1px solid #e2e8f0;text-align:center;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">© 2026 {{CompanyName}} · Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string expenseSubmittedHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f3f6fb;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:28px 14px;"">
    <table width=""620"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#0f172a 0%,#1e3a8a 100%);padding:28px 36px;"">
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""><tr>
            <td>
              <p style=""margin:0 0 8px;color:#bfdbfe;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
              <h1 style=""margin:0;color:#ffffff;font-size:24px;font-weight:700;line-height:1.3;"">Expense or Advance Request Submitted</h1>
              <p style=""margin:8px 0 0;color:#dbeafe;font-size:13px;line-height:1.6;"">A new request has entered the workflow and is ready for core-team visibility.</p>
            </td>
            <td align=""right"" style=""vertical-align:top;"">
              <div style=""display:inline-block;padding:8px 12px;border:1px solid rgba(255,255,255,.22);border-radius:999px;color:#eff6ff;font-size:12px;font-weight:600;"">{{CurrentStatus}}</div>
            </td>
          </tr></table>
        </td>
      </tr>
      <tr>
        <td style=""padding:30px 36px;"">
          <p style=""margin:0 0 18px;color:#334155;font-size:14px;line-height:1.75;"">Request <strong>{{RequestNumber}}</strong> was submitted by <strong>{{EmployeeName}}</strong>. The summary is below for review and tracking.</p>

          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:separate;border-spacing:0 12px;margin-bottom:22px;"">
            <tr>
              <td width=""50%"" style=""padding-right:8px;vertical-align:top;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border:1px solid #dbe3f0;border-radius:12px;"">
                  <tr><td style=""padding:18px 20px;"">
                    <div style=""margin:0 0 12px;color:#0f172a;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;"">Request Snapshot</div>
                    <table width=""100%"" cellpadding=""5"" cellspacing=""0"" style=""color:#334155;font-size:13px;"">
                      <tr><td style=""color:#64748b;width:46%;border-bottom:1px solid #e2e8f0;"">Request No</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{RequestNumber}}</td></tr>
                      <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Submitted By</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{EmployeeName}}</td></tr>
                      <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Employee Code</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{EmployeeCode}}</td></tr>
                      <tr><td style=""color:#64748b;"">Submitted On</td><td style=""font-weight:700;"">{{SubmittedAt}}</td></tr>
                    </table>
                  </td></tr>
                </table>
              </td>
              <td width=""50%"" style=""padding-left:8px;vertical-align:top;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8fafc;border:1px solid #dbe3f0;border-radius:12px;"">
                  <tr><td style=""padding:18px 20px;"">
                    <div style=""margin:0 0 12px;color:#0f172a;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;"">Review Context</div>
                    <table width=""100%"" cellpadding=""5"" cellspacing=""0"" style=""color:#334155;font-size:13px;"">
                      <tr><td style=""color:#64748b;width:46%;border-bottom:1px solid #e2e8f0;"">Current Status</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{CurrentStatus}}</td></tr>
                      <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Approver</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{ApproverName}}</td></tr>
                      <tr><td style=""color:#64748b;border-bottom:1px solid #e2e8f0;"">Line Items</td><td style=""font-weight:700;border-bottom:1px solid #e2e8f0;"">{{ItemCount}}</td></tr>
                      <tr><td style=""color:#64748b;"">Total Amount</td><td style=""font-weight:700;color:#0f766e;"">{{TotalAmount}}</td></tr>
                    </table>
                  </td></tr>
                </table>
              </td>
            </tr>
          </table>

          <div style=""background:#eff6ff;border:1px solid #bfdbfe;border-radius:12px;padding:16px 18px;margin-bottom:24px;"">
            <div style=""margin:0 0 8px;color:#1d4ed8;font-size:12px;font-weight:700;text-transform:uppercase;letter-spacing:.05em;"">Purpose</div>
            <p style=""margin:0;color:#1e293b;font-size:14px;line-height:1.75;"">{{PurposeOfTravel}}</p>
          </div>

          <a href=""{{DetailsUrl}}"" style=""display:inline-block;background:#0f172a;color:#ffffff;text-decoration:none;padding:12px 18px;border-radius:10px;font-size:13px;font-weight:700;"">Open Request Details</a>
          <p style=""margin:16px 0 0;color:#64748b;font-size:12px;line-height:1.7;"">You are receiving this notification because you are marked as a core member in {{AppName}}.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:14px 36px;border-top:1px solid #e2e8f0;text-align:center;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">© 2026 {{CompanyName}} · Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string extendedHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""580"" cellpadding=""0"" cellspacing=""0""
           style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#2563eb 0%,#1d4ed8 100%);padding:36px 40px;text-align:center;"">
          <p style=""margin:0;color:#bfdbfe;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:8px 0 4px;color:#fff;font-size:26px;font-weight:700;"">Licence Renewed &#10003;</h1>
          <p style=""margin:0;color:#dbeafe;font-size:14px;"">Your {{AppName}} licence expiry has been extended</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:36px 40px;"">
          <p style=""margin:0 0 16px;color:#374151;font-size:15px;"">Dear <strong>{{ClientName}}</strong>,</p>
          <p style=""margin:0 0 24px;color:#374151;font-size:14px;line-height:1.7;"">
            Your <strong>{{AppName}}</strong> licence (<strong>{{ClientCode}}</strong>) has been successfully renewed.
            You can continue using the application without any interruption.
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                 style=""background:#eff6ff;border-radius:10px;border:1px solid #bfdbfe;margin-bottom:24px;"">
            <tr><td style=""padding:20px 24px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"">
                <tr>
                  <td style=""color:#6b7280;font-size:13px;width:45%;border-bottom:1px solid #dbeafe;"">Client Code</td>
                  <td style=""color:#1e293b;font-size:13px;font-weight:700;border-bottom:1px solid #dbeafe;"">{{ClientCode}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;"">New Expiry Date</td>
                  <td style=""color:#1d4ed8;font-size:14px;font-weight:700;"">{{ExpiryDate}}</td>
                </tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0;color:#9ca3af;font-size:13px;"">Thank you for continuing with {{CompanyName}}.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #f1f5f9;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 {{CompanyName}} &middot; Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string licenceExpiryHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""580"" cellpadding=""0"" cellspacing=""0""
           style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#d97706 0%,#b45309 100%);padding:36px 40px;text-align:center;"">
          <p style=""margin:0;color:#fef3c7;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:8px 0 4px;color:#fff;font-size:26px;font-weight:700;"">&#9203; Licence Expiry Reminder</h1>
          <p style=""margin:0;color:#fef3c7;font-size:14px;"">Your {{AppName}} licence expires in {{DaysRemaining}} day(s)</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:36px 40px;"">
          <p style=""margin:0 0 16px;color:#374151;font-size:15px;"">Dear <strong>{{ClientName}}</strong>,</p>
          <p style=""margin:0 0 24px;color:#374151;font-size:14px;line-height:1.7;"">
            This is a friendly reminder that your <strong>{{AppName}}</strong> licence
            (<strong>{{ClientCode}}</strong>) is expiring on <strong style=""color:#d97706;"">{{ExpiryDate}}</strong>.
            Please renew your licence to avoid service interruption.
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                 style=""background:#fffbeb;border-radius:10px;border:1px solid #fde68a;margin-bottom:24px;"">
            <tr><td style=""padding:20px 24px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"">
                <tr>
                  <td style=""color:#6b7280;font-size:13px;width:45%;border-bottom:1px solid #fde68a;"">Client Code</td>
                  <td style=""color:#1e293b;font-size:13px;font-weight:700;border-bottom:1px solid #fde68a;"">{{ClientCode}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;border-bottom:1px solid #fde68a;"">Expiry Date</td>
                  <td style=""color:#b45309;font-size:14px;font-weight:700;border-bottom:1px solid #fde68a;"">{{ExpiryDate}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;"">Days Remaining</td>
                  <td style=""color:#b45309;font-size:15px;font-weight:800;"">{{DaysRemaining}} day(s)</td>
                </tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0;color:#9ca3af;font-size:13px;"">Contact your support representative to renew your licence today.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #f1f5f9;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 {{CompanyName}} &middot; Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string amcExpiryHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#f1f5f9;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:40px 20px;"">
    <table width=""580"" cellpadding=""0"" cellspacing=""0""
           style=""background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 16px rgba(0,0,0,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#dc2626 0%,#b91c1c 100%);padding:36px 40px;text-align:center;"">
          <p style=""margin:0;color:#fecaca;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:8px 0 4px;color:#fff;font-size:26px;font-weight:700;"">&#9888; AMC Expiry Reminder</h1>
          <p style=""margin:0;color:#fecaca;font-size:14px;"">Your Annual Maintenance Contract expires in {{DaysRemaining}} day(s)</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:36px 40px;"">
          <p style=""margin:0 0 16px;color:#374151;font-size:15px;"">Dear <strong>{{ClientName}}</strong>,</p>
          <p style=""margin:0 0 24px;color:#374151;font-size:14px;line-height:1.7;"">
            Your Annual Maintenance Contract (AMC) for <strong>{{AppName}}</strong>
            (<strong>{{ClientCode}}</strong>) is due to expire on
            <strong style=""color:#dc2626;"">{{AmcExpiryDate}}</strong>.
            Renewing your AMC ensures you continue to receive software updates, technical support,
            and priority service.
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                 style=""background:#fef2f2;border-radius:10px;border:1px solid #fecaca;margin-bottom:24px;"">
            <tr><td style=""padding:20px 24px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"">
                <tr>
                  <td style=""color:#6b7280;font-size:13px;width:45%;border-bottom:1px solid #fecaca;"">Client Code</td>
                  <td style=""color:#1e293b;font-size:13px;font-weight:700;border-bottom:1px solid #fecaca;"">{{ClientCode}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;border-bottom:1px solid #fecaca;"">AMC Expiry Date</td>
                  <td style=""color:#b91c1c;font-size:14px;font-weight:700;border-bottom:1px solid #fecaca;"">{{AmcExpiryDate}}</td>
                </tr>
                <tr>
                  <td style=""color:#6b7280;font-size:13px;"">Days Remaining</td>
                  <td style=""color:#b91c1c;font-size:15px;font-weight:800;"">{{DaysRemaining}} day(s)</td>
                </tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0;color:#9ca3af;font-size:13px;"">Contact your support representative to renew your AMC today.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #f1f5f9;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 {{CompanyName}} &middot; Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            const string userOnboardingHtml = @"<!DOCTYPE html><html><head><meta charset=""utf-8""/></head>
<body style=""margin:0;padding:0;background:#eef2ff;font-family:Segoe UI,Arial,sans-serif;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"">
  <tr><td align=""center"" style=""padding:32px 16px;"">
    <table width=""620"" cellpadding=""0"" cellspacing=""0""
           style=""background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 8px 24px rgba(15,23,42,.08);"">
      <tr>
        <td style=""background:linear-gradient(135deg,#180a3c 0%,#2d145f 100%);padding:34px 40px;text-align:center;"">
          <p style=""margin:0;color:#d6ccff;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">{{CompanyName}}</p>
          <h1 style=""margin:10px 0 6px;color:#fff;font-size:28px;font-weight:700;"">Welcome to {{AppName}}</h1>
          <p style=""margin:0;color:#ebe7ff;font-size:14px;line-height:1.6;"">Your user account has been created successfully.</p>
        </td>
      </tr>
      <tr>
        <td style=""padding:34px 40px;"">
          <p style=""margin:0 0 14px;color:#334155;font-size:15px;"">Dear <strong>{{FullName}}</strong>,</p>
          <p style=""margin:0 0 22px;color:#475569;font-size:14px;line-height:1.75;"">
            Welcome aboard. Your profile has been onboarded in <strong>{{AppName}}</strong>. The account summary is shared below for your reference.
          </p>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0""
                 style=""background:#f8faff;border:1px solid #dbe4ff;border-radius:12px;margin-bottom:20px;"">
            <tr><td style=""padding:20px 22px;"">
              <table width=""100%"" cellpadding=""6"" cellspacing=""0"">
                <tr><td style=""width:42%;color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Username</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{Username}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Email</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{Email}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Full Name</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{FullName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Phone Number</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{PhoneNumber}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Role</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{RoleName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Location</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{LocationName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Department</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{DepartmentName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Designation</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{DesignationName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Employee Code</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{EmployeeCode}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Manager</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{ManagerName}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;border-bottom:1px solid #e2e8f0;"">Core Member</td><td style=""color:#0f172a;font-size:13px;font-weight:700;border-bottom:1px solid #e2e8f0;"">{{IsCoreMember}}</td></tr>
                <tr><td style=""color:#64748b;font-size:13px;"">Status</td><td style=""color:#0f172a;font-size:13px;font-weight:700;"">{{Status}}</td></tr>
              </table>
            </td></tr>
          </table>
          <p style=""margin:0 0 10px;color:#475569;font-size:14px;line-height:1.7;"">
            Login URL: <a href=""{{LoginUrl}}"" style=""color:#4338ca;font-weight:600;text-decoration:none;"">{{LoginUrl}}</a>
          </p>
          <p style=""margin:0;color:#94a3b8;font-size:12px;line-height:1.7;"">
            If any of the above information is incorrect, please contact your administrator.
          </p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:15px 40px;text-align:center;border-top:1px solid #e2e8f0;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">© 2026 {{CompanyName}} · Tech Driven HealthCare</p>
        </td>
      </tr>
    </table>
  </td></tr>
</table>
</body></html>";

            var templates = new[]
            {
                ("LICENSE_ACTIVATED",       "License Activation — Welcome Again",
                 "Welcome Back! Your {{AppName}} Licence is Active — {{ClientCode}}",
                 activatedHtml),
                ("LICENSE_EXPIRY_EXTENDED", "License Expiry Extended",
                 "Your {{AppName}} Licence Has Been Renewed — {{ClientCode}}",
                 extendedHtml),
                ("LICENSE_EXPIRY_REMINDER", "License Expiry Reminder (7 Days)",
                 "\u23f0 Licence Expiry Reminder — {{DaysRemaining}} Day(s) Left — {{ClientCode}}",
                 licenceExpiryHtml),
                ("AMC_EXPIRY_REMINDER",     "AMC Expiry Reminder (7 Days)",
                 "\u26a0\ufe0f AMC Expiry Reminder — {{DaysRemaining}} Day(s) Left — {{ClientCode}}",
                 amcExpiryHtml),
                ("USER_ONBOARDING",         "New User Onboarding",
                 "Welcome to {{AppName}} - Your Account Details",
                  userOnboardingHtml),
                 ("EXPENSE_REQUEST_SUBMITTED", "Expense Request Submitted",
                  "Expense Request {{RequestNumber}} Submitted by {{EmployeeName}}",
                  expenseSubmittedHtml),
                 ("EXPENSE_SETTLEMENT_COMPLETED", "Expense Settlement Completed",
                  "Settlement completed for request {{RequestNumber}}",
                  settlementCompletedHtml)
            };

            foreach (var (key, name, subject, body) in templates)
            {
                var exists = await conn.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM tbl_centralemailtemplates WHERE TemplateKey = @Key",
                    new { Key = key });
                if (!exists)
                {
                    await conn.ExecuteAsync(@"
                        INSERT INTO tbl_centralemailtemplates
                            (TemplateKey, TemplateName, Subject, Body, IsActive, CreatedAt)
                        VALUES (@Key, @Name, @Subject, @Body, 1, GETDATE())",
                        new { Key = key, Name = name, Subject = subject, Body = body });
                }
            }
        }
    }
}

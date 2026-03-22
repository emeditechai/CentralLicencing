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
                await EnsureUserMasterAsync(conn);
                await SeedDefaultUsersAsync(conn);
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
                    INSERT INTO RoleMaster(RoleName,Description,IsActive) VALUES('Staff','Read-only access',1);";
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
                        [RoleId]        INT            NOT NULL REFERENCES [dbo].[RoleMaster]([Id]),
                        [IsActive]      BIT            NOT NULL DEFAULT 1,
                        [CreatedAt]     DATETIME       NOT NULL DEFAULT GETDATE(),
                        [LastLoginDate] DATETIME       NULL
                    );
                END";
            await conn.ExecuteAsync(sql);
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

            if (!await conn.ExecuteScalarAsync<bool>("SELECT COUNT(1) FROM UserMaster WHERE Username='staff'"))
            {
                var hash = BCrypt.Net.BCrypt.HashPassword("Staff@1234");
                await conn.ExecuteAsync(@"
                    INSERT INTO UserMaster(Username,Email,PasswordHash,FullName,RoleId,IsActive,CreatedAt)
                    VALUES('staff','staff@centrallicence.com',@Hash,'Staff Member',@RoleId,1,GETDATE())",
                    new { Hash = hash, RoleId = staffRoleId });
            }
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
          <p style=""margin:0;color:#c7d2fe;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">Emeditech Plus LLP</p>
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
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 Emeditech Plus LLP &middot; Tech Driven HealthCare</p>
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
          <p style=""margin:0;color:#bfdbfe;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">Emeditech Plus LLP</p>
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
          <p style=""margin:0;color:#9ca3af;font-size:13px;"">Thank you for continuing with Emeditech Plus LLP.</p>
        </td>
      </tr>
      <tr>
        <td style=""background:#f8fafc;padding:16px 40px;text-align:center;border-top:1px solid #f1f5f9;"">
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 Emeditech Plus LLP &middot; Tech Driven HealthCare</p>
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
          <p style=""margin:0;color:#fef3c7;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">Emeditech Plus LLP</p>
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
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 Emeditech Plus LLP &middot; Tech Driven HealthCare</p>
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
          <p style=""margin:0;color:#fecaca;font-size:12px;letter-spacing:2px;text-transform:uppercase;"">Emeditech Plus LLP</p>
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
          <p style=""margin:0;color:#94a3b8;font-size:12px;"">&#169; 2026 Emeditech Plus LLP &middot; Tech Driven HealthCare</p>
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
                 amcExpiryHtml)
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

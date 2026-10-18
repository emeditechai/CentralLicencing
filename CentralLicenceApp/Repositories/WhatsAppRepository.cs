using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class WhatsAppRepository : IWhatsAppRepository
    {
        private readonly string _connectionString;

        public WhatsAppRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task EnsureTablesAsync()
        {
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "084_CreateWhatsAppTables.sql");
            if (!File.Exists(scriptPath)) return;
            using var conn = CreateConnection();
            var sql = await File.ReadAllTextAsync(scriptPath);
            foreach (var batch in sql.Split(new[] { "\nGO\n", "\nGO\r\n", "\r\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = batch.Trim();
                if (trimmed.EndsWith("\nGO")) trimmed = trimmed[..^2];
                if (!string.IsNullOrWhiteSpace(trimmed))
                    await conn.ExecuteAsync(trimmed);
            }
        }

        public async Task<WhatsAppConfiguration?> GetConfigAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<WhatsAppConfiguration>(
                "SELECT TOP 1 * FROM WhatsAppConfiguration ORDER BY Id");
        }

        public async Task<int> SaveConfigAsync(WhatsAppConfiguration config, string user)
        {
            using var conn = CreateConnection();
            if (config.Id == 0)
            {
                config.CreatedBy = user;
                return await conn.ExecuteScalarAsync<int>(@"
                    INSERT INTO WhatsAppConfiguration
                        (ConfigName, ApiUrl, AuthHeaderKey, ApiKeyEncrypted, ApiKeyLast4, DefaultCountryCode,
                         SenderPhone, IsEnabled, NotifyOnEmail, MinGapMs, CreatedBy, CreatedDate)
                    VALUES
                        (@ConfigName, @ApiUrl, @AuthHeaderKey, @ApiKeyEncrypted, @ApiKeyLast4, @DefaultCountryCode,
                         @SenderPhone, @IsEnabled, @NotifyOnEmail, @MinGapMs, @CreatedBy, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", config);
            }

            config.ModifiedBy = user;
            await conn.ExecuteAsync(@"
                UPDATE WhatsAppConfiguration SET
                    ConfigName         = @ConfigName,
                    ApiUrl             = @ApiUrl,
                    AuthHeaderKey      = @AuthHeaderKey,
                    ApiKeyEncrypted    = @ApiKeyEncrypted,
                    ApiKeyLast4        = @ApiKeyLast4,
                    DefaultCountryCode = @DefaultCountryCode,
                    SenderPhone        = @SenderPhone,
                    IsEnabled          = @IsEnabled,
                    NotifyOnEmail      = @NotifyOnEmail,
                    MinGapMs           = @MinGapMs,
                    ModifiedBy         = @ModifiedBy,
                    ModifiedDate       = GETDATE()
                WHERE Id = @Id", config);
            return config.Id;
        }

        public async Task UpdateTestResultAsync(int configId, string result)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE WhatsAppConfiguration SET LastTestedDate = GETDATE(), LastTestResult = @Result WHERE Id = @Id",
                new { Id = configId, Result = result.Length > 500 ? result[..500] : result });
        }

        public async Task InsertLogAsync(WhatsAppLog log)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO WhatsAppLog
                    (ConfigId, ModuleCode, ModuleRefId, RecipientPhone, MessageText, IsSuccess,
                     MsgId, Jid, Status, ApiResponse, ErrorMessage, SentBy, SentDate)
                VALUES
                    (@ConfigId, @ModuleCode, @ModuleRefId, @RecipientPhone, @MessageText, @IsSuccess,
                     @MsgId, @Jid, @Status, @ApiResponse, @ErrorMessage, @SentBy, GETDATE())", log);
        }

        public async Task<IEnumerable<(string Email, string PhoneNumber)>> GetUserPhonesByEmailAsync(IEnumerable<string> emails)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<(string Email, string PhoneNumber)>(@"
                SELECT Email, PhoneNumber FROM UserMaster
                WHERE Email IN @Emails AND IsActive = 1
                  AND PhoneNumber IS NOT NULL AND LTRIM(RTRIM(PhoneNumber)) <> ''", new { Emails = emails });
        }

        public async Task<IEnumerable<WhatsAppLog>> GetRecentLogsAsync(int count)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<WhatsAppLog>(
                "SELECT TOP (@Count) * FROM WhatsAppLog ORDER BY SentDate DESC, Id DESC", new { Count = count });
        }
    }
}

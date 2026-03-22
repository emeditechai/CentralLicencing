using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly string _connectionString;

        public ReminderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<bool> WasSentTodayAsync(int licenseId, string reminderType)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(@"
                SELECT COUNT(1) FROM tbl_centralemailreminders
                WHERE LicenseId = @LicenseId
                  AND ReminderType = @ReminderType
                  AND CAST(SentAt AS DATE) = CAST(GETDATE() AS DATE)",
                new { LicenseId = licenseId, ReminderType = reminderType });
            return count > 0;
        }

        public async Task RecordAsync(int licenseId, string reminderType, string toEmail)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(@"
                INSERT INTO tbl_centralemailreminders (LicenseId, ReminderType, SentAt, ToEmail)
                VALUES (@LicenseId, @ReminderType, GETDATE(), @ToEmail)",
                new { LicenseId = licenseId, ReminderType = reminderType, ToEmail = toEmail });
        }
    }
}

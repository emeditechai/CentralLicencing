using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class EmailLogRepository : IEmailLogRepository
    {
        private readonly string _connectionString;

        public EmailLogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> CreateAsync(EmailLogEntry entry)
        {
            using var conn = CreateConnection();
            const string sql = @"
                INSERT INTO tbl_centralemaillog
                    (EmailType, TemplateKey, RecipientEmail, RecipientName, Subject, Body, Status, ErrorMessage, TriggeredBy, CreatedAt)
                VALUES
                    (@EmailType, @TemplateKey, @RecipientEmail, @RecipientName, @Subject, @Body, @Status, @ErrorMessage, @TriggeredBy, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            entry.CreatedAt = entry.CreatedAt == default ? DateTime.Now : entry.CreatedAt;
            return await conn.ExecuteScalarAsync<int>(sql, entry);
        }

        public async Task<(IEnumerable<EmailLogEntry> Items, int TotalCount)> GetPagedAsync(
            DateTime? fromDate, DateTime? toDate, string? emailType, int page, int pageSize)
        {
            using var conn = CreateConnection();

            var where = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (fromDate.HasValue)
            {
                where += " AND l.CreatedAt >= @FromDate";
                parameters.Add("FromDate", fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                where += " AND l.CreatedAt < @ToDateExclusive";
                parameters.Add("ToDateExclusive", toDate.Value.Date.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(emailType))
            {
                where += " AND l.EmailType = @EmailType";
                parameters.Add("EmailType", emailType);
            }

            var countSql = $"SELECT COUNT(*) FROM tbl_centralemaillog l {where}";
            var dataSql = $@"
                SELECT l.*
                FROM tbl_centralemaillog l
                {where}
                ORDER BY l.CreatedAt DESC, l.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await conn.QueryAsync<EmailLogEntry>(dataSql, parameters);
            return (items, totalCount);
        }

        public async Task<IEnumerable<string>> GetDistinctEmailTypesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<string>(@"
                SELECT DISTINCT EmailType
                FROM tbl_centralemaillog
                WHERE NULLIF(LTRIM(RTRIM(EmailType)), '') IS NOT NULL
                ORDER BY EmailType");
        }
    }
}
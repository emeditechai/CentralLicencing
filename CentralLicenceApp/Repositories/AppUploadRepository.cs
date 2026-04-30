using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class AppUploadRepository : IAppUploadRepository
    {
        private readonly string _connectionString;

        public AppUploadRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<AppUploadLog>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AppUploadLog>(
                "SELECT * FROM AppUploadLog ORDER BY UploadedAt DESC");
        }

        public async Task<AppUploadLog?> GetLatestByPlatformAsync(string platform)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<AppUploadLog>(
                @"SELECT TOP 1 * FROM AppUploadLog
                  WHERE Platform = @Platform
                  ORDER BY UploadedAt DESC",
                new { Platform = platform });
        }

        public async Task<int> AddAsync(AppUploadLog log)
        {
            using var conn = CreateConnection();
            log.UploadedAt = DateTime.Now;
            var sql = @"
                INSERT INTO AppUploadLog
                    (Platform, FileName, OriginalName, FileSizeBytes, DownloadUrl, UploadedBy, UploadedAt, Notes)
                VALUES
                    (@Platform, @FileName, @OriginalName, @FileSizeBytes, @DownloadUrl, @UploadedBy, @UploadedAt, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, log);
        }
    }
}

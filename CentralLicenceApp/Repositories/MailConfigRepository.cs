using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class MailConfigRepository : IMailConfigRepository
    {
        private readonly string _connectionString;

        public MailConfigRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<MailConfiguration>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<MailConfiguration>(
                "SELECT * FROM tbl_centralmailconfiguration ORDER BY CreatedAt DESC");
        }

        public async Task<MailConfiguration?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<MailConfiguration>(
                "SELECT * FROM tbl_centralmailconfiguration WHERE Id = @Id", new { Id = id });
        }

        public async Task<MailConfiguration?> GetActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<MailConfiguration>(
                "SELECT TOP 1 * FROM tbl_centralmailconfiguration WHERE IsActive = 1 ORDER BY Id");
        }

        public async Task<int> CreateAsync(MailConfiguration config, string createdBy)
        {
            using var conn = CreateConnection();
            var sql = @"
                INSERT INTO tbl_centralmailconfiguration
                    (SmtpServer, SmtpPort, SmtpUsername, SmtpPassword, EnableSSL,
                     FromEmail, FromName, AdminNotificationEmail, IsActive, CreatedAt, CreatedBy)
                VALUES
                    (@SmtpServer, @SmtpPort, @SmtpUsername, @SmtpPassword, @EnableSSL,
                     @FromEmail, @FromName, @AdminNotificationEmail, @IsActive, @CreatedAt, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            config.CreatedAt = DateTime.Now;
            config.CreatedBy = createdBy;
            return await conn.ExecuteScalarAsync<int>(sql, config);
        }

        public async Task UpdateAsync(MailConfiguration config, string updatedBy)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE tbl_centralmailconfiguration SET
                    SmtpServer              = @SmtpServer,
                    SmtpPort                = @SmtpPort,
                    SmtpUsername            = @SmtpUsername,
                    SmtpPassword            = @SmtpPassword,
                    EnableSSL               = @EnableSSL,
                    FromEmail               = @FromEmail,
                    FromName                = @FromName,
                    AdminNotificationEmail  = @AdminNotificationEmail,
                    IsActive                = @IsActive,
                    UpdatedAt               = @UpdatedAt,
                    UpdatedBy               = @UpdatedBy
                WHERE Id = @Id";
            config.UpdatedAt = DateTime.Now;
            config.UpdatedBy = updatedBy;
            await conn.ExecuteAsync(sql, config);
        }

        public async Task SetActiveAsync(int id, bool isActive)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync(
                "UPDATE tbl_centralmailconfiguration SET IsActive = @IsActive WHERE Id = @Id",
                new { IsActive = isActive, Id = id });
        }
    }
}

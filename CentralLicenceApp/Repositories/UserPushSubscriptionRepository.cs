using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class UserPushSubscriptionRepository : IUserPushSubscriptionRepository
    {
        private readonly string _connectionString;

        public UserPushSubscriptionRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task UpsertAsync(UserPushSubscription subscription)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            await conn.ExecuteAsync(@"
MERGE dbo.UserPushSubscription AS target
USING (SELECT @Endpoint AS Endpoint) AS source
ON target.Endpoint = source.Endpoint
WHEN MATCHED THEN
    UPDATE SET
        UserId = @UserId,
        P256dh = @P256dh,
        Auth = @Auth,
        UserAgent = @UserAgent,
        IsActive = 1,
        UpdatedAt = @UpdatedAt
WHEN NOT MATCHED THEN
    INSERT (UserId, Endpoint, P256dh, Auth, UserAgent, IsActive, CreatedAt, UpdatedAt)
    VALUES (@UserId, @Endpoint, @P256dh, @Auth, @UserAgent, 1, @CreatedAt, @UpdatedAt);",
                subscription);
        }

        public async Task DeactivateAsync(int userId, string endpoint)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            await conn.ExecuteAsync(@"
                UPDATE dbo.UserPushSubscription
                SET IsActive = 0,
                    UpdatedAt = @UpdatedAt
                WHERE UserId = @UserId
                  AND Endpoint = @Endpoint",
                new
                {
                    UserId = userId,
                    Endpoint = endpoint,
                    UpdatedAt = DateTime.Now
                });
        }

        public async Task DeactivateByEndpointAsync(string endpoint)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            await conn.ExecuteAsync(@"
                UPDATE dbo.UserPushSubscription
                SET IsActive = 0,
                    UpdatedAt = @UpdatedAt
                WHERE Endpoint = @Endpoint",
                new
                {
                    Endpoint = endpoint,
                    UpdatedAt = DateTime.Now
                });
        }

        public async Task<IEnumerable<UserPushSubscription>> GetActiveByUserIdsAsync(IEnumerable<int> userIds)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            return await conn.QueryAsync<UserPushSubscription>(@"
                SELECT Id, UserId, Endpoint, P256dh, Auth, UserAgent, IsActive, CreatedAt, UpdatedAt
                FROM dbo.UserPushSubscription
                WHERE IsActive = 1
                  AND UserId IN @UserIds",
                new { UserIds = userIds });
        }
    }
}
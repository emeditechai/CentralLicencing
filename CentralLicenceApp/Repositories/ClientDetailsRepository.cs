using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ClientDetailsRepository : IClientDetailsRepository
    {
        private readonly string _connectionString;

        public ClientDetailsRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<ClientDetails?> GetByClientCodeAsync(string clientCode)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ClientDetails>(
                @"SELECT ID, ClientCode, ClientPersonName, address AS Address,
                         ProductPurchased, DOB, Anniversarydate, IsActive
                  FROM ClientDetails WHERE ClientCode = @ClientCode",
                new { ClientCode = clientCode });
        }

        public async Task UpsertAsync(ClientDetails details)
        {
            using var conn = CreateConnection();

            var existing = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM ClientDetails WHERE ClientCode = @ClientCode",
                new { details.ClientCode });

            if (existing == 0)
            {
                await conn.ExecuteAsync(@"
                    INSERT INTO ClientDetails
                        (ClientCode, ClientPersonName, address, ProductPurchased,
                         DOB, Anniversarydate, IsActive)
                    VALUES
                        (@ClientCode, @ClientPersonName, @Address, @ProductPurchased,
                         @DOB, @Anniversarydate, @IsActive)",
                    details);
            }
            else
            {
                await conn.ExecuteAsync(@"
                    UPDATE ClientDetails SET
                        ClientPersonName  = @ClientPersonName,
                        address           = @Address,
                        ProductPurchased  = @ProductPurchased,
                        DOB               = @DOB,
                        Anniversarydate   = @Anniversarydate,
                        IsActive          = @IsActive
                    WHERE ClientCode = @ClientCode",
                    details);
            }
        }

        public async Task<IEnumerable<string>> GetClientCodesWithDetailsAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<string>(
                "SELECT ClientCode FROM ClientDetails WHERE ClientCode IS NOT NULL");
        }
    }
}

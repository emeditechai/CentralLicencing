using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PricingModelRepository : IPricingModelRepository
    {
        private readonly string _connectionString;

        public PricingModelRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<PricingModelMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PricingModelMaster>(
                "SELECT * FROM PricingModelMaster ORDER BY ModelName");
        }

        public async Task<IEnumerable<PricingModelMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PricingModelMaster>(
                "SELECT * FROM PricingModelMaster WHERE IsActive = 1 ORDER BY ModelName");
        }

        public async Task<bool> ExistsActiveAsync(string modelName)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM PricingModelMaster
                    WHERE ModelName = @ModelName AND IsActive = 1
                ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                new { ModelName = modelName });
        }
    }
}
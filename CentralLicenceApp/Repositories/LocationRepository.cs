using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly string _connectionString;

        public LocationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<LocationMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LocationMaster>(
                "SELECT * FROM LocationMaster WHERE IsActive = 1 ORDER BY Name");
        }
    }
}

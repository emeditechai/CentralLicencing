using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PermissionMasterRepository : IPermissionMasterRepository
    {
        private readonly string _connectionString;
        public PermissionMasterRepository(string c) { _connectionString = c; }
        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<PermissionMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PermissionMaster>(
                "SELECT * FROM PermissionMaster WHERE IsActive=1 ORDER BY SortOrder, PermissionKey");
        }

        public async Task<PermissionMaster?> GetByKeyAsync(string key)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<PermissionMaster>(
                "SELECT * FROM PermissionMaster WHERE PermissionKey=@K", new { K = key });
        }
    }
}

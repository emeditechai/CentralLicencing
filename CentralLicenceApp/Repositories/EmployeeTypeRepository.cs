using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class EmployeeTypeRepository : IEmployeeTypeRepository
    {
        private readonly string _connectionString;

        public EmployeeTypeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<EmployeeTypeMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeTypeMaster>(
                "SELECT * FROM EmployeeTypeMaster ORDER BY TypeName");
        }

        public async Task<IEnumerable<EmployeeTypeMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeTypeMaster>(
                "SELECT * FROM EmployeeTypeMaster WHERE IsActive = 1 ORDER BY TypeName");
        }

        public async Task<EmployeeTypeMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmployeeTypeMaster>(
                "SELECT * FROM EmployeeTypeMaster WHERE Id = @Id", new { Id = id });
        }
    }
}
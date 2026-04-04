using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PaymentModeRepository : IPaymentModeRepository
    {
        private readonly string _connectionString;

        public PaymentModeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<PaymentMode>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PaymentMode>(
                "SELECT * FROM PaymentMode ORDER BY SortOrder, Name");
        }

        public async Task<IEnumerable<PaymentMode>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PaymentMode>(
                "SELECT * FROM PaymentMode WHERE IsActive = 1 ORDER BY SortOrder, Name");
        }

        public async Task<PaymentMode?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<PaymentMode>(
                "SELECT * FROM PaymentMode WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(PaymentMode mode)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO PaymentMode (Name, IsActive, SortOrder)
                VALUES (@Name, @IsActive, @SortOrder);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                mode);
        }

        public async Task<bool> UpdateAsync(PaymentMode mode)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE PaymentMode
                SET Name = @Name, SortOrder = @SortOrder
                WHERE Id = @Id",
                mode);
            return rows > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE PaymentMode
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE Id = @Id",
                new { Id = id });
            return rows > 0;
        }
    }
}

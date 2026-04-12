using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TaskTypeMasterRepository : ITaskTypeMasterRepository
    {
        private readonly string _connectionString;

        public TaskTypeMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TaskTypeMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TaskTypeMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskTypeMaster ORDER BY Name");
        }

        public async Task<IEnumerable<TaskTypeMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TaskTypeMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskTypeMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<TaskTypeMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TaskTypeMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskTypeMaster WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CreateAsync(TaskTypeMaster item)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO TaskTypeMaster (Name, IsActive)
                VALUES (@Name, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { item.Name });
        }

        public async Task<bool> UpdateAsync(TaskTypeMaster item)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE TaskTypeMaster SET Name = @Name WHERE Id = @Id",
                new { item.Id, item.Name }) > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE TaskTypeMaster
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE Id = @Id",
                new { Id = id }) > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM DailyTaskLog WHERE TaskTypeId = @Id",
                new { Id = id });
            return count > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TaskTypeMaster WHERE Id = @Id",
                new { Id = id }) > 0;
        }
    }
}

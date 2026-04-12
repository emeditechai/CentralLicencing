using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TaskCategoryMasterRepository : ITaskCategoryMasterRepository
    {
        private readonly string _connectionString;

        public TaskCategoryMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TaskCategoryMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TaskCategoryMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskCategoryMaster ORDER BY Name");
        }

        public async Task<IEnumerable<TaskCategoryMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TaskCategoryMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskCategoryMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<TaskCategoryMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TaskCategoryMaster>(
                "SELECT Id, Name, IsActive, CreatedAt FROM TaskCategoryMaster WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CreateAsync(TaskCategoryMaster item)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO TaskCategoryMaster (Name, IsActive)
                VALUES (@Name, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { item.Name });
        }

        public async Task<bool> UpdateAsync(TaskCategoryMaster item)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE TaskCategoryMaster SET Name = @Name WHERE Id = @Id",
                new { item.Id, item.Name }) > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE TaskCategoryMaster
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE Id = @Id",
                new { Id = id }) > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM DailyTaskLog WHERE TaskCategoryId = @Id",
                new { Id = id });
            return count > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TaskCategoryMaster WHERE Id = @Id",
                new { Id = id }) > 0;
        }
    }
}

using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ProjectModuleRepository : IProjectModuleRepository
    {
        private readonly string _connectionString;

        public ProjectModuleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ProjectModuleMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProjectModuleMaster>(
                "SELECT Id, Name, Description, IsActive, CreatedAt FROM ProjectModuleMaster ORDER BY Name");
        }

        public async Task<IEnumerable<ProjectModuleMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ProjectModuleMaster>(
                "SELECT Id, Name, Description, IsActive, CreatedAt FROM ProjectModuleMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<ProjectModuleMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ProjectModuleMaster>(
                "SELECT Id, Name, Description, IsActive, CreatedAt FROM ProjectModuleMaster WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CreateAsync(ProjectModuleMaster project)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(@"
                INSERT INTO ProjectModuleMaster (Name, Description, IsActive)
                VALUES (@Name, @Description, 1);
                SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { project.Name, project.Description });
        }

        public async Task<bool> UpdateAsync(ProjectModuleMaster project)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE ProjectModuleMaster
                SET Name = @Name, Description = @Description
                WHERE Id = @Id",
                new { project.Id, project.Name, project.Description }) > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(@"
                UPDATE ProjectModuleMaster
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE Id = @Id",
                new { Id = id }) > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM DailyTaskLog WHERE ProjectModuleId = @Id",
                new { Id = id });
            return count > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM ProjectModuleMaster WHERE Id = @Id",
                new { Id = id }) > 0;
        }
    }
}

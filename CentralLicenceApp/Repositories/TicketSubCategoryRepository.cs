using System.Data;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TicketSubCategoryRepository : ITicketSubCategoryRepository
    {
        private readonly string _connectionString;

        public TicketSubCategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<TicketSubCategoryMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketSubCategoryMaster>(@"
                SELECT sc.*, c.CategoryName
                FROM TicketSubCategoryMaster sc
                INNER JOIN TicketCategoryMaster c ON c.Id = sc.CategoryId
                ORDER BY c.CategoryName, sc.SubCategoryName");
        }

        public async Task<IEnumerable<TicketSubCategoryMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketSubCategoryMaster>(@"
                SELECT sc.*, c.CategoryName
                FROM TicketSubCategoryMaster sc
                INNER JOIN TicketCategoryMaster c ON c.Id = sc.CategoryId
                WHERE sc.IsActive = 1 AND c.IsActive = 1
                ORDER BY c.CategoryName, sc.SubCategoryName");
        }

        public async Task<IEnumerable<TicketSubCategoryMaster>> GetByCategoryIdAsync(int categoryId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<TicketSubCategoryMaster>(@"
                SELECT sc.*, c.CategoryName
                FROM TicketSubCategoryMaster sc
                INNER JOIN TicketCategoryMaster c ON c.Id = sc.CategoryId
                WHERE sc.CategoryId = @CategoryId AND sc.IsActive = 1
                ORDER BY sc.SubCategoryName",
                new { CategoryId = categoryId });
        }

        public async Task<TicketSubCategoryMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<TicketSubCategoryMaster>(@"
                SELECT sc.*, c.CategoryName
                FROM TicketSubCategoryMaster sc
                INNER JOIN TicketCategoryMaster c ON c.Id = sc.CategoryId
                WHERE sc.Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(TicketSubCategoryMaster subCategory)
        {
            using var conn = CreateConnection();
            subCategory.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO TicketSubCategoryMaster (CategoryId, SubCategoryName, Description, IsActive, CreatedAt)
                VALUES (@CategoryId, @SubCategoryName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, subCategory);
        }

        public async Task<bool> UpdateAsync(TicketSubCategoryMaster subCategory)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE TicketSubCategoryMaster SET
                    CategoryId      = @CategoryId,
                    SubCategoryName = @SubCategoryName,
                    Description     = @Description,
                    IsActive        = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, subCategory) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var count = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM HelpDeskTicket WHERE SubCategoryId = @Id", new { Id = id });

            return count == 0
                ? (true, null)
                : (false, $"This sub-category cannot be deleted because {count} ticket(s) reference it.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM TicketSubCategoryMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

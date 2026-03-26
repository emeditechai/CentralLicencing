using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ExpenseCategoryRepository : IExpenseCategoryRepository
    {
        private readonly string _connectionString;

        public ExpenseCategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ExpenseCategoryMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ExpenseCategoryMaster>(
                "SELECT * FROM ExpenseCategoryMaster ORDER BY CategoryName");
        }

        public async Task<IEnumerable<ExpenseCategoryMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ExpenseCategoryMaster>(
                "SELECT * FROM ExpenseCategoryMaster WHERE IsActive = 1 ORDER BY CategoryName");
        }

        public async Task<ExpenseCategoryMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ExpenseCategoryMaster>(
                "SELECT * FROM ExpenseCategoryMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(ExpenseCategoryMaster expenseCategory)
        {
            using var conn = CreateConnection();
            expenseCategory.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO ExpenseCategoryMaster (CategoryName, Description, IsActive, CreatedAt)
                VALUES (@CategoryName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, expenseCategory);
        }

        public async Task<bool> UpdateAsync(ExpenseCategoryMaster expenseCategory)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE ExpenseCategoryMaster SET
                    CategoryName = @CategoryName,
                    Description = @Description,
                    IsActive = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, expenseCategory) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM ExpenseCategoryMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class EmployeeDesignationRepository : IEmployeeDesignationRepository
    {
        private readonly string _connectionString;

        public EmployeeDesignationRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<EmployeeDesignationMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDesignationMaster>(
                "SELECT * FROM EmployeeDesignationMaster ORDER BY DesignationName");
        }

        public async Task<IEnumerable<EmployeeDesignationMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDesignationMaster>(
                "SELECT * FROM EmployeeDesignationMaster WHERE IsActive = 1 ORDER BY DesignationName");
        }

        public async Task<EmployeeDesignationMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmployeeDesignationMaster>(
                "SELECT * FROM EmployeeDesignationMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(EmployeeDesignationMaster designation)
        {
            using var conn = CreateConnection();
            designation.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO EmployeeDesignationMaster (DesignationName, Description, IsActive, CreatedAt)
                VALUES (@DesignationName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, designation);
        }

        public async Task<bool> UpdateAsync(EmployeeDesignationMaster designation)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE EmployeeDesignationMaster SET
                    DesignationName = @DesignationName,
                    Description     = @Description,
                    IsActive        = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, designation) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var linkedUserCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM UserMaster WHERE DesignationId = @Id",
                new { Id = id });

            if (linkedUserCount == 0)
            {
                return (true, null);
            }

            return (false, $"This designation cannot be deleted because it is assigned to {linkedUserCount} user account(s). Reassign those users first.");
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM EmployeeDesignationMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
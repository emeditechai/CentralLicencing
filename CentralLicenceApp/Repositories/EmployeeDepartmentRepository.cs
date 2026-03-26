using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class EmployeeDepartmentRepository : IEmployeeDepartmentRepository
    {
        private readonly string _connectionString;

        public EmployeeDepartmentRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<EmployeeDepartmentMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDepartmentMaster>(
                "SELECT * FROM EmployeeDepartmentMaster ORDER BY DepartmentName");
        }

        public async Task<IEnumerable<EmployeeDepartmentMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<EmployeeDepartmentMaster>(
                "SELECT * FROM EmployeeDepartmentMaster WHERE IsActive = 1 ORDER BY DepartmentName");
        }

        public async Task<EmployeeDepartmentMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<EmployeeDepartmentMaster>(
                "SELECT * FROM EmployeeDepartmentMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(EmployeeDepartmentMaster department)
        {
            using var conn = CreateConnection();
            department.CreatedAt = DateTime.Now;
            var sql = @"
                INSERT INTO EmployeeDepartmentMaster (DepartmentName, Description, IsActive, CreatedAt)
                VALUES (@DepartmentName, @Description, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, department);
        }

        public async Task<bool> UpdateAsync(EmployeeDepartmentMaster department)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE EmployeeDepartmentMaster SET
                    DepartmentName = @DepartmentName,
                    Description    = @Description,
                    IsActive       = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, department) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM EmployeeDepartmentMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
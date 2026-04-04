using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class BankMasterRepository : IBankMasterRepository
    {
        private readonly string _connectionString;

        public BankMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<BankMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<BankMaster>(
                "SELECT * FROM BankMaster ORDER BY IsPrimary DESC, BankName");
        }

        public async Task<BankMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<BankMaster>(
                "SELECT * FROM BankMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<BankMaster?> GetPrimaryAsync()
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<BankMaster>(
                "SELECT TOP 1 * FROM BankMaster WHERE IsPrimary = 1 AND IsActive = 1");
        }

        public async Task<int> CreateAsync(BankMaster bank)
        {
            using var conn = CreateConnection();
            bank.CreatedAt = DateTime.Now;

            const string sql = @"
                INSERT INTO BankMaster (BankName, AccountNumber, BranchName, IFSCCode, UpiId, UpiHolderName, IsPrimary, IsActive, CreatedAt)
                VALUES (@BankName, @AccountNumber, @BranchName, @IFSCCode, @UpiId, @UpiHolderName, @IsPrimary, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, bank);
        }

        public async Task<bool> UpdateAsync(BankMaster bank)
        {
            using var conn = CreateConnection();

            const string sql = @"
                UPDATE BankMaster SET
                    BankName       = @BankName,
                    AccountNumber  = @AccountNumber,
                    BranchName     = @BranchName,
                    IFSCCode       = @IFSCCode,
                    UpiId          = @UpiId,
                    UpiHolderName  = @UpiHolderName,
                    IsPrimary      = @IsPrimary,
                    IsActive       = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, bank) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM BankMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

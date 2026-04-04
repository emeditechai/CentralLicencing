using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class PartyMasterRepository : IPartyMasterRepository
    {
        private readonly string _connectionString;

        public PartyMasterRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<PartyMaster>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PartyMaster>(
                "SELECT * FROM PartyMaster ORDER BY PartyName");
        }

        public async Task<IEnumerable<PartyMaster>> GetAllActiveAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<PartyMaster>(
                "SELECT * FROM PartyMaster WHERE IsActive = 1 ORDER BY PartyName");
        }

        public async Task<PartyMaster?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<PartyMaster>(
                "SELECT * FROM PartyMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(PartyMaster party)
        {
            using var conn = CreateConnection();
            party.CreatedAt = DateTime.Now;
            const string sql = @"
                INSERT INTO PartyMaster (PartyName, ContactPerson, Mobile, Email, Address, GSTINNo, PANNo, IsActive, CreatedAt)
                VALUES (@PartyName, @ContactPerson, @Mobile, @Email, @Address, @GSTINNo, @PANNo, @IsActive, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, party);
        }

        public async Task<bool> UpdateAsync(PartyMaster party)
        {
            using var conn = CreateConnection();
            const string sql = @"
                UPDATE PartyMaster SET
                    PartyName     = @PartyName,
                    ContactPerson = @ContactPerson,
                    Mobile        = @Mobile,
                    Email         = @Email,
                    Address       = @Address,
                    GSTINNo       = @GSTINNo,
                    PANNo         = @PANNo,
                    IsActive      = @IsActive
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, party) > 0;
        }

        public async Task<(bool CanDelete, string? Reason)> ValidateDeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var quotationCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Quotation WHERE PartyId = @Id", new { Id = id });
            if (quotationCount > 0)
                return (false, $"This party cannot be deleted because it is linked to {quotationCount} quotation(s).");

            var invoiceCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Invoice WHERE PartyId = @Id", new { Id = id });
            if (invoiceCount > 0)
                return (false, $"This party cannot be deleted because it is linked to {invoiceCount} invoice(s).");

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.ExecuteAsync(
                "DELETE FROM PartyMaster WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}

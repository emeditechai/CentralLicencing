using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ClientLicenseAuditLogRepository : IClientLicenseAuditLogRepository
    {
        private readonly string _connectionString;

        public ClientLicenseAuditLogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task AddAsync(ClientLicenseAuditLog entry)
        {
            using var conn = CreateConnection();
            entry.ChangedAt = DateTime.Now;
            await conn.ExecuteAsync(@"
                INSERT INTO ClientLicenseAuditLog
                    (ClientLicenseId, ClientCode, ClientName, ProductType, FieldChanged, OldValue, NewValue, ChangedBy, ChangedAt)
                VALUES
                    (@ClientLicenseId, @ClientCode, @ClientName, @ProductType, @FieldChanged, @OldValue, @NewValue, @ChangedBy, @ChangedAt)",
                entry);
        }

        public async Task<(IEnumerable<ClientLicenseAuditLog> Items, int TotalCount)> GetPagedAsync(
            string? search, string? field, int page, int pageSize)
        {
            using var conn = CreateConnection();

            var where = new StringBuilder("WHERE 1=1");
            var p     = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(search))
            {
                where.Append(@" AND (
                    ClientCode  LIKE @Search OR
                    ClientName  LIKE @Search OR
                    ProductType LIKE @Search OR
                    ChangedBy   LIKE @Search)");
                p.Add("Search", $"%{search.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(field))
            {
                where.Append(" AND FieldChanged = @Field");
                p.Add("Field", field.Trim());
            }

            var countSql = $"SELECT COUNT(1) FROM ClientLicenseAuditLog {where}";
            var total    = await conn.ExecuteScalarAsync<int>(countSql, p);

            p.Add("Skip", (page - 1) * pageSize);
            p.Add("Take", pageSize);

            var dataSql = $@"
                SELECT * FROM ClientLicenseAuditLog {where}
                ORDER BY ChangedAt DESC
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY";

            var items = await conn.QueryAsync<ClientLicenseAuditLog>(dataSql, p);
            return (items, total);
        }

        public async Task<IEnumerable<ClientLicenseAuditLog>> GetByLicenseIdAsync(int licenseId)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ClientLicenseAuditLog>(
                "SELECT * FROM ClientLicenseAuditLog WHERE ClientLicenseId = @Id ORDER BY ChangedAt DESC",
                new { Id = licenseId });
        }
    }
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public class LicenseHistoryRepository : ILicenseHistoryRepository
    {
        private readonly string _connectionString;

        public LicenseHistoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<LicenseValidationHistory>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LicenseValidationHistory>(
                "SELECT * FROM LicenseValidationHistory ORDER BY CreatedAt DESC");
        }

        public async Task<(IEnumerable<LicenseValidationHistory> Items, int TotalCount)> GetPagedAsync(
            string? clientCode, string? validFilter, string? productType, int page, int pageSize)
        {
            using var conn = CreateConnection();

            var where = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(productType))
            {
                where += " AND EXISTS (SELECT 1 FROM ClientAppLicense c WHERE c.ClientCode = h.ClientCode AND c.ProductType = @ProductType)";
                parameters.Add("ProductType", productType);
            }

            if (!string.IsNullOrWhiteSpace(clientCode))
            {
                where += " AND (h.ClientCode LIKE @ClientCode OR h.LicenseKey LIKE @ClientCode)";
                parameters.Add("ClientCode", $"%{clientCode}%");
            }

            if (validFilter == "valid")
                where += " AND h.IsValid = 1";
            else if (validFilter == "invalid")
                where += " AND h.IsValid = 0";

            var countSql = $"SELECT COUNT(*) FROM LicenseValidationHistory h {where}";
            var dataSql = $@"
                SELECT h.* FROM LicenseValidationHistory h {where}
                ORDER BY h.CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await conn.QueryAsync<LicenseValidationHistory>(dataSql, parameters);

            return (items, totalCount);
        }

        public async Task<IEnumerable<LicenseValidationHistory>> GetByClientCodeAsync(string clientCode)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<LicenseValidationHistory>(
                "SELECT * FROM LicenseValidationHistory WHERE ClientCode = @Code ORDER BY CreatedAt DESC",
                new { Code = clientCode });
        }
    }
}

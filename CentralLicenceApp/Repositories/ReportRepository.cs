using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models.Reports;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IReadOnlyList<ClientDetailsReportRow>> GetClientDetailsReportAsync(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<ClientDetailsReportRow>(
                "dbo.usp_Report_ClientDetails",
                new
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    ProductType = string.IsNullOrWhiteSpace(productType) ? null : productType.Trim()
                },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }
    }
}
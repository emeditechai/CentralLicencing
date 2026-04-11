using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public class ClientLicenseRepository : IClientLicenseRepository
    {
        private readonly string _connectionString;

        public ClientLicenseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ClientAppLicense>> GetAllAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ClientAppLicense>(
                "SELECT * FROM ClientAppLicense ORDER BY CreatedAt DESC");
        }

        public async Task<(IEnumerable<ClientAppLicense> Items, int TotalCount)> GetPagedAsync(
            string? search, string? status, string? productType, int page, int pageSize)
        {
            using var conn = CreateConnection();

            var where = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(productType))
            {
                where += " AND ProductType = @ProductType";
                parameters.Add("ProductType", productType);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                where += " AND (ClientName LIKE @Search OR ClientCode LIKE @Search OR EmailID LIKE @Search OR ContactNumber LIKE @Search)";
                parameters.Add("Search", $"%{search}%");
            }

            if (status == "active")
            {
                where += " AND IsActive = 1 AND ExpiryDate >= GETDATE()";
            }
            else if (status == "expired")
            {
                where += " AND ExpiryDate < GETDATE()";
            }
            else if (status == "inactive")
            {
                where += " AND IsActive = 0";
            }

            var countSql = $"SELECT COUNT(*) FROM ClientAppLicense {where}";
            var dataSql = $@"
                SELECT * FROM ClientAppLicense {where}
                ORDER BY CreatedAt DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            parameters.Add("Offset", (page - 1) * pageSize);
            parameters.Add("PageSize", pageSize);

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            var items = await conn.QueryAsync<ClientAppLicense>(dataSql, parameters);

            return (items, totalCount);
        }

        public async Task<ClientAppLicense?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ClientAppLicense>(
                "SELECT * FROM ClientAppLicense WHERE Id = @Id", new { Id = id });
        }

        public async Task<ClientAppLicense?> GetByClientCodeAsync(string clientCode)
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleOrDefaultAsync<ClientAppLicense>(
                "SELECT * FROM ClientAppLicense WHERE ClientCode = @Code", new { Code = clientCode });
        }

        public async Task<int> CreateAsync(ClientAppLicense license)
        {
            using var conn = CreateConnection();

            // Auto-generate client code: CL-YYMM + sequence
            var year = DateTime.Now.ToString("yy");
            var month = DateTime.Now.ToString("MM");
            var prefix = $"Cl-{year}{month}";
            var countSql = $"SELECT COUNT(*) FROM ClientAppLicense WHERE ClientCode LIKE '{prefix}%'";
            var count = await conn.ExecuteScalarAsync<int>(countSql);
            license.ClientCode = $"{prefix}{(count + 1):D4}";
            license.LicenseKey = Guid.NewGuid().ToString().ToUpper();
            license.Startdate = DateTime.Now;
            license.CreatedAt = DateTime.Now;

            var sql = @"
                INSERT INTO ClientAppLicense
                    (ClientCode, ClientName, ContactNumber, LicenseKey, HardDiskNumber,
                     ServerMacID, MotherboardNumber, Startdate, ExpiryDate, IsActive,
                     CreatedAt, OTP_Verified, PublicIPAddress, EmailID, AMC_Expireddate, AppUrl, ProductType)
                VALUES
                    (@ClientCode, @ClientName, @ContactNumber, @LicenseKey, @HardDiskNumber,
                     @ServerMacID, @MotherboardNumber, @Startdate, @ExpiryDate, @IsActive,
                     @CreatedAt, @OTP_Verified, @PublicIPAddress, @EmailID, @AMC_Expireddate, @AppUrl, @ProductType);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await conn.ExecuteScalarAsync<int>(sql, license);
        }

        public async Task<bool> UpdateAsync(ClientAppLicense license)
        {
            using var conn = CreateConnection();
            var sql = @"
                UPDATE ClientAppLicense SET
                    ClientName       = @ClientName,
                    ContactNumber    = @ContactNumber,
                    ExpiryDate       = @ExpiryDate,
                    IsActive         = @IsActive,
                    EmailID          = @EmailID,
                    AMC_Expireddate  = @AMC_Expireddate,
                    AppUrl           = @AppUrl,
                    ProductType      = @ProductType,
                    ConnectionString = @ConnectionString
                WHERE Id = @Id";
            var rows = await conn.ExecuteAsync(sql, license);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM ClientAppLicense WHERE Id = @Id", new { Id = id });
            return rows > 0;
        }

        public async Task<IEnumerable<ClientAppLicense>> GetLicensesExpiringWithinDaysAsync(int days)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ClientAppLicense>(@"
                SELECT * FROM ClientAppLicense
                WHERE IsActive = 1
                  AND EmailID IS NOT NULL AND EmailID <> ''
                  AND ExpiryDate >= CAST(GETDATE() AS DATE)
                  AND ExpiryDate <= DATEADD(DAY, @Days, CAST(GETDATE() AS DATE))",
                new { Days = days });
        }

        public async Task<IEnumerable<ClientAppLicense>> GetAmcExpiringWithinDaysAsync(int days)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<ClientAppLicense>(@"
                SELECT * FROM ClientAppLicense
                WHERE EmailID IS NOT NULL AND EmailID <> ''
                  AND AMC_Expireddate IS NOT NULL
                  AND AMC_Expireddate >= CAST(GETDATE() AS DATE)
                  AND AMC_Expireddate <= DATEADD(DAY, @Days, CAST(GETDATE() AS DATE))",
                new { Days = days });
        }

        public async Task<IEnumerable<string>> GetDistinctProductTypesAsync()
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<string>(
                "SELECT DISTINCT ProductType FROM ClientAppLicense WHERE ProductType IS NOT NULL AND ProductType <> '' ORDER BY ProductType");
        }

        public async Task<DashboardViewModel> GetDashboardStatsAsync(string? productType = null)
        {
            using var conn = CreateConnection();

            var parameters = new DynamicParameters();
            var licWhere   = "";
            var histExists = "";

            if (!string.IsNullOrWhiteSpace(productType))
            {
                parameters.Add("ProductType", productType);
                licWhere   = " AND ProductType = @ProductType";
                histExists = " AND EXISTS (SELECT 1 FROM ClientAppLicense c WHERE c.ClientCode = h.ClientCode AND c.ProductType = @ProductType)";
            }

            var sql = $@"
                SELECT
                    (SELECT COUNT(*) FROM ClientAppLicense WHERE 1=1{licWhere}) AS TotalClients,
                    (SELECT COUNT(*) FROM ClientAppLicense WHERE IsActive = 1 AND ExpiryDate >= GETDATE(){licWhere}) AS ActiveLicenses,
                    (SELECT COUNT(*) FROM ClientAppLicense WHERE ExpiryDate < GETDATE(){licWhere}) AS ExpiredLicenses,
                    (SELECT COUNT(*) FROM ClientAppLicense WHERE IsActive = 1 AND ExpiryDate >= GETDATE() AND ExpiryDate < DATEADD(MONTH,1,GETDATE()){licWhere}) AS ExpiringThisMonth,
                    (SELECT COUNT(*) FROM LicenseValidationHistory h WHERE 1=1{histExists}) AS TotalValidations,
                    (SELECT COUNT(*) FROM LicenseValidationHistory h WHERE h.IsValid = 0{histExists}) AS FailedValidations,
                    (SELECT COUNT(*) FROM LicenseValidationHistory h WHERE CAST(h.CreatedAt AS DATE) = CAST(GETDATE() AS DATE){histExists}) AS TodayValidations;

                SELECT TOP 5 * FROM ClientAppLicense WHERE 1=1{licWhere} ORDER BY CreatedAt DESC;

                SELECT TOP 10 * FROM ClientAppLicense
                WHERE IsActive = 1 AND ExpiryDate >= GETDATE(){licWhere}
                ORDER BY ExpiryDate ASC;

                SELECT
                    FORMAT(h.CreatedAt,'MMM yyyy') AS Month,
                    SUM(CASE WHEN h.IsValid = 1 THEN 1 ELSE 0 END) AS ValidCount,
                    SUM(CASE WHEN h.IsValid = 0 THEN 1 ELSE 0 END) AS InvalidCount
                FROM LicenseValidationHistory h
                WHERE h.CreatedAt >= DATEADD(MONTH, -5, GETDATE()){histExists}
                GROUP BY FORMAT(h.CreatedAt,'MMM yyyy'), YEAR(h.CreatedAt), MONTH(h.CreatedAt)
                ORDER BY YEAR(h.CreatedAt), MONTH(h.CreatedAt);";

            using var multi = await conn.QueryMultipleAsync(sql, string.IsNullOrWhiteSpace(productType) ? null : (object)parameters);
            var stats           = await multi.ReadSingleAsync<dynamic>();
            var recentClients   = (await multi.ReadAsync<ClientAppLicense>()).ToList();
            var upcomingExpiries= (await multi.ReadAsync<ClientAppLicense>()).ToList();
            var monthlyStats    = (await multi.ReadAsync<MonthlyValidationStat>()).ToList();

            return new DashboardViewModel
            {
                TotalClients      = (int)stats.TotalClients,
                ActiveLicenses    = (int)stats.ActiveLicenses,
                ExpiredLicenses   = (int)stats.ExpiredLicenses,
                ExpiringThisMonth = (int)stats.ExpiringThisMonth,
                TotalValidations  = (int)stats.TotalValidations,
                FailedValidations = (int)stats.FailedValidations,
                TodayValidations  = (int)stats.TodayValidations,
                RecentClients     = recentClients,
                UpcomingExpiries  = upcomingExpiries,
                MonthlyStats      = monthlyStats,
                ValidationStatusStats = new List<ValidationStatusStat>
                {
                    new() { Label = "Valid",   Count = (int)stats.TotalValidations - (int)stats.FailedValidations },
                    new() { Label = "Invalid", Count = (int)stats.FailedValidations }
                }
            };
        }

        public async Task<bool> UpdateMaintenanceAlertAsync(int id, bool isDisplayAlerts, DateTime? alertStartDate, TimeSpan? alertStartTime, DateTime? alertEndDate, TimeSpan? alertEndTime, string? alertMessage)
        {
            using var conn = CreateConnection();
            var rows = await conn.ExecuteAsync(@"
                UPDATE ClientAppLicense
                SET IsDisplayAlerts = @IsDisplayAlerts,
                    AlertStartDate  = @AlertStartDate,
                    AlertStartTime  = @AlertStartTime,
                    AlertEndDate    = @AlertEndDate,
                    AlertEndTime    = @AlertEndTime,
                    AlertMessage    = @AlertMessage
                WHERE Id = @Id",
                new { Id = id, IsDisplayAlerts = isDisplayAlerts, AlertStartDate = alertStartDate, AlertStartTime = alertStartTime, AlertEndDate = alertEndDate, AlertEndTime = alertEndTime, AlertMessage = alertMessage });
            return rows > 0;
        }
    }
}

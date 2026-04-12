using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models.Reports;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class TaskReportRepository : ITaskReportRepository
    {
        private readonly string _connectionString;

        public TaskReportRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<(IReadOnlyList<TimesheetReportRow> Items, int TotalCount)> GetTimesheetReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, int? taskTypeId, int page, int pageSize)
        {
            using var conn = CreateConnection();
            var items = (await conn.QueryAsync<TimesheetReportRow>(
                "dbo.usp_Report_Timesheet",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, TaskTypeId = taskTypeId, Page = page, PageSize = pageSize },
                commandType: CommandType.StoredProcedure)).ToList();

            var totalCount = items.FirstOrDefault()?.TotalCount ?? 0;
            return (items, totalCount);
        }

        public async Task<IReadOnlyList<EmployeeProductivityRow>> GetEmployeeProductivityReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<EmployeeProductivityRow>(
                "dbo.usp_Report_EmployeeProductivity",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task<IReadOnlyList<ProjectEffortRow>> GetProjectEffortReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, int? projectModuleId)
        {
            using var conn = CreateConnection();
            var items = await conn.QueryAsync<ProjectEffortRow>(
                "dbo.usp_Report_ProjectEffort",
                new { FromDate = fromDate, ToDate = toDate, UserId = userId, ProjectModuleId = projectModuleId },
                commandType: CommandType.StoredProcedure);

            return items.ToList();
        }

        public async Task EnsureProceduresAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "063_TaskReportStoredProcedures.sql");
            if (!File.Exists(scriptPath)) return;
            var sql = await File.ReadAllTextAsync(scriptPath);
            foreach (var batch in sql.Split(new[] { "\nGO\n", "\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(batch))
                    await conn.ExecuteAsync(batch);
            }
        }
    }
}

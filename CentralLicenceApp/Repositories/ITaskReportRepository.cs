using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Repositories
{
    public interface ITaskReportRepository
    {
        // Timesheet
        Task<(IReadOnlyList<TimesheetReportRow> Items, int TotalCount)> GetTimesheetReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, int? taskTypeId, int page, int pageSize);

        // Employee Productivity
        Task<IReadOnlyList<EmployeeProductivityRow>> GetEmployeeProductivityReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId);

        // Project / Module Effort
        Task<IReadOnlyList<ProjectEffortRow>> GetProjectEffortReportAsync(
            DateTime? fromDate, DateTime? toDate, int? userId, int? projectModuleId);

        // Bootstrap stored procedures
        Task EnsureProceduresAsync();
    }
}

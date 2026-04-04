using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models.Reports;

namespace CentralLicenceApp.Repositories
{
    public interface IReportRepository
    {
        Task<IReadOnlyList<ClientDetailsReportRow>> GetClientDetailsReportAsync(DateTime? fromDate, DateTime? toDate, string? productType);
        Task<(IReadOnlyList<ExpenseReportRow> Items, int TotalCount)> GetExpenseReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int page, int pageSize);
        Task<IReadOnlyList<ExpenseReportRow>> GetAllExpenseReportAsync(DateTime? fromDate, DateTime? toDate, int? userId);
        Task<(IReadOnlyList<SettlementReportRow> Items, int TotalCount)> GetSettlementReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int page, int pageSize);
        Task<IReadOnlyList<SettlementReportRow>> GetAllSettlementReportAsync(DateTime? fromDate, DateTime? toDate, int? userId);
    }
}
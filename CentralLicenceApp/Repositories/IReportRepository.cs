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

        Task<(IReadOnlyList<DailyCollectionRow> Items, int TotalCount)> GetDailyCollectionReportAsync(DateTime? fromDate, DateTime? toDate, string? collectedBy, int page, int pageSize);
        Task<IReadOnlyList<DailyCollectionRow>> GetAllDailyCollectionReportAsync(DateTime? fromDate, DateTime? toDate, string? collectedBy);
        Task<(IReadOnlyList<ClientDueRow> Items, int TotalCount)> GetClientDueReportAsync(DateTime? fromDate, DateTime? toDate, int page, int pageSize);
        Task<IReadOnlyList<ClientDueRow>> GetAllClientDueReportAsync(DateTime? fromDate, DateTime? toDate);
    }
}
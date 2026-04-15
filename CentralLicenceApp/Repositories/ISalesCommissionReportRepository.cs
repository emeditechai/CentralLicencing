using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ISalesCommissionReportRepository
    {
        Task<IEnumerable<SalesCommSummaryRow>> GetSummaryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId);
        Task<IEnumerable<SalesCommDetailRow>> GetDetailReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int? batchId);
        Task<IEnumerable<SalesCommHistoryRow>> GetHistoryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, string? status);
    }
}

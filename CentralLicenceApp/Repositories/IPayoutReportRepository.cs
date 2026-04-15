using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IPayoutReportRepository
    {
        Task<IEnumerable<PayoutSummaryRow>> GetSummaryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId);
        Task<IEnumerable<PayoutDetailRow>> GetDetailReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, int? batchId);
        Task<IEnumerable<PayoutHistoryRow>> GetHistoryReportAsync(DateTime? fromDate, DateTime? toDate, int? userId, string? status);
        Task EnsureStoredProceduresAsync();
    }
}

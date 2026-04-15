using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IPayoutBatchRepository
    {
        // Batches
        Task<IEnumerable<PayoutBatch>> GetBatchesAsync(int? userId, string? statusFilter, DateTime? fromDate, DateTime? toDate);
        Task<PayoutBatch?> GetBatchByIdAsync(int id);
        Task<IEnumerable<PayoutBatchLine>> GetBatchLinesAsync(int batchId);
        Task<IEnumerable<PayoutApprovalHistory>> GetApprovalHistoryAsync(int batchId);
        Task<bool> DeleteBatchAsync(int id);

        // Generation
        Task<PayoutPreviewResult?> PreviewEligibleTasksAsync(int userId, DateTime fromDate, DateTime toDate);
        Task<int> GenerateBatchAsync(int userId, DateTime fromDate, DateTime toDate, string? remarks, int generatedById);

        // Workflow
        Task<bool> SubmitForApprovalAsync(int batchId);
        Task<bool> ApproveOrRejectAsync(int batchId, int approverLevel, int approvedById, string status, string? remarks);
        Task<IEnumerable<PayoutBatch>> GetPendingApprovalsAsync(int approverLevel, int? approverUserId);

        // Settlement
        Task<bool> SettleBatchAsync(PayoutSettlementFormViewModel model, int settledById);
    }
}

using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface ISalesCommissionBatchRepository
    {
        // Batches
        Task<IEnumerable<SalesCommissionBatch>> GetBatchesAsync(int? userId, string? statusFilter, DateTime? fromDate, DateTime? toDate);
        Task<SalesCommissionBatch?> GetBatchByIdAsync(int id);
        Task<IEnumerable<SalesCommissionBatchLine>> GetBatchLinesAsync(int batchId);
        Task<IEnumerable<SalesCommissionApprovalHistory>> GetApprovalHistoryAsync(int batchId);
        Task<bool> DeleteBatchAsync(int id);

        // Generation
        Task<SalesCommPreviewResult?> PreviewEligiblePaymentsAsync(int userId, DateTime fromDate, DateTime toDate);
        Task<int> GenerateBatchAsync(int userId, DateTime fromDate, DateTime toDate, string? remarks, int generatedById);
        Task<BulkGenerateResult> GenerateBulkAsync(DateTime fromDate, DateTime toDate, int generatedById);

        // Workflow
        Task<bool> SubmitForApprovalAsync(int batchId);
        Task<bool> ApproveOrRejectAsync(int batchId, int approverLevel, int approvedById, string status, string? remarks);
        Task<IEnumerable<SalesCommissionBatch>> GetPendingApprovalsAsync(int approverLevel, int? approverUserId);

        // Settlement
        Task<bool> SettleBatchAsync(SalesCommSettlementFormViewModel model, int settledById);
    }
}

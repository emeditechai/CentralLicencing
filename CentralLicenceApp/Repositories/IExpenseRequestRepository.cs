using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;

namespace CentralLicenceApp.Repositories
{
    public interface IExpenseRequestRepository
    {
        Task<IEnumerable<ExpenseRequest>> GetRequestsForEmployeeAsync(int employeeId);
        Task<IEnumerable<ExpenseRequest>> GetPendingApprovalsAsync(int approverId);
        Task<IEnumerable<ExpenseRequest>> GetFinanceQueueAsync();
        Task<(int Approved, int ReimbursementInProcess, int Settled)> GetDashboardCountsAsync();
        Task<IEnumerable<ExpenseRequest>> GetAllAsync();
        Task<ExpenseRequest?> GetByIdAsync(int id);
        Task<IEnumerable<ExpenseRequestLine>> GetLinesAsync(int requestId);
        Task<ExpenseRequestLine?> GetLineByIdAsync(int lineId);
        Task<IEnumerable<ExpenseRequestLineAttachment>> GetAttachmentsForLineAsync(int lineId);
        Task<IEnumerable<ExpenseRequestApprovalHistory>> GetHistoryAsync(int requestId);
        Task<int> CreateDraftAsync(ExpenseRequest request);
        Task<bool> UpdateDraftAsync(ExpenseRequest request);
        Task<bool> DeleteDraftAsync(int id, int employeeId);
        Task<int> AddLineAsync(ExpenseRequestLine line);
        Task<bool> UpdateLineAsync(ExpenseRequestLine line);
        Task AddLineAttachmentsAsync(int lineId, IEnumerable<ExpenseRequestLineAttachment> attachments);
        Task<bool> DeleteLineAsync(int lineId, int requestId);
        Task<bool> SubmitAsync(int requestId, int employeeId, int? approverId, bool autoApprove, string? remarks);
        Task<bool> ApproveAsync(int requestId, int actionByUserId, string? remarks);
        Task<bool> RejectAsync(int requestId, int actionByUserId, string? remarks);
        Task<bool> StartReimbursementAsync(int requestId, int actionByUserId, string remarks);
        Task<bool> SettleAsync(int requestId, int actionByUserId, DateTime settlementDate, decimal settlementAmount, string settlementMode, string settlementReferenceNo, string? settlementRemarks);
    }
}
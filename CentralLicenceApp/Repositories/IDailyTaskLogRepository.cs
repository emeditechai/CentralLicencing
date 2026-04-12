using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;

namespace CentralLicenceApp.Repositories
{
    public interface IDailyTaskLogRepository
    {
        // Task CRUD
        Task<IEnumerable<DailyTaskLog>> GetTasksAsync(int userId, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, string? search);
        Task<IEnumerable<DailyTaskLog>> GetAssignedTasksAsync(int userId, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, string? search);
        Task<IEnumerable<DailyTaskLog>> GetTeamTasksAsync(IEnumerable<int> userIds, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, int? userFilter, string? search);
        Task<DailyTaskLog?> GetByIdAsync(int id);
        Task<int> CreateAsync(DailyTaskLog task);
        Task<bool> UpdateAsync(DailyTaskLog task);
        Task<bool> UpdateTaskStatusAsync(int id, string status);
        Task<bool> DeleteAsync(int id);

        // Time Log CRUD
        Task<IEnumerable<TaskTimeLog>> GetTimeLogsAsync(int taskId);
        Task<TaskTimeLog?> GetTimeLogByIdAsync(int id);
        Task<int> AddTimeLogAsync(TaskTimeLog entry);
        Task<bool> DeleteTimeLogAsync(int id);

        // Masters
        Task<IEnumerable<TaskTypeMaster>> GetTaskTypesAsync();
        Task<IEnumerable<TaskCategoryMaster>> GetTaskCategoriesAsync();
        Task<IEnumerable<ProjectModuleMaster>> GetProjectsAsync();
        Task<ProjectModuleMaster?> GetProjectByIdAsync(int id);
        Task<int> CreateProjectAsync(ProjectModuleMaster project);
        Task<bool> UpdateProjectAsync(ProjectModuleMaster project);

        // Ticket lookup
        Task<IEnumerable<(int Id, string TicketNumber, string Subject)>> SearchTicketsAsync(string term);

        // Assignable users (Ticket Admin + Ticket Agent roles)
        Task<IEnumerable<UserMaster>> GetAssignableUsersAsync();

        // Summary / Dashboard
        Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
              int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetSummaryAsync(int userId, DateTime? from, DateTime? to);
        Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
              int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetAssignedSummaryAsync(int userId, DateTime? from, DateTime? to);
        Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
              int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetTeamSummaryAsync(IEnumerable<int> userIds, DateTime? from, DateTime? to);

        // Ensure tables exist
        Task EnsureTablesAsync();
    }
}

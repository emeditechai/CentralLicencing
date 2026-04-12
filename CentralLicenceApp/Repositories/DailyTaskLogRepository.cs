using System.Data;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Repositories
{
    public class DailyTaskLogRepository : IDailyTaskLogRepository
    {
        private readonly string _connectionString;

        public DailyTaskLogRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        private const string BaseSelectSql = @"
            SELECT t.Id, t.UserId, t.AssignedToUserId, t.TaskDate, t.TaskTypeId, t.TaskCategoryId,
                   t.TaskTitle, t.Description, t.TicketId, t.ProjectModuleId,
                   t.TimeSpentMinutes, t.Status, t.CreatedAt, t.UpdatedAt,
                   u.FullName AS UserName,
                   au.FullName AS AssignedToUserName,
                   tt.Name AS TaskTypeName,
                   tc.Name AS TaskCategoryName,
                   tk.TicketNumber, tk.Subject AS TicketSubject,
                   pm.Name AS ProjectModuleName,
                   ISNULL(tlog.TotalTime, 0) AS TotalTimeSpentMinutes,
                   tlog.LastLogDate,
                   ISNULL(tlog.LogCount, 0) AS TimeLogCount
            FROM DailyTaskLog t
            INNER JOIN UserMaster u ON u.Id = t.UserId
            LEFT JOIN UserMaster au ON au.Id = t.AssignedToUserId
            INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
            INNER JOIN TaskCategoryMaster tc ON tc.Id = t.TaskCategoryId
            LEFT JOIN HelpDeskTicket tk ON tk.Id = t.TicketId
            LEFT JOIN ProjectModuleMaster pm ON pm.Id = t.ProjectModuleId
            OUTER APPLY (
                SELECT SUM(TimeSpentMinutes) AS TotalTime,
                       MAX(LogDate) AS LastLogDate,
                       COUNT(*) AS LogCount
                FROM TaskTimeLog WHERE TaskId = t.Id
            ) tlog";

        public async Task<IEnumerable<DailyTaskLog>> GetTasksAsync(int userId, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, string? search)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            var sql = BaseSelectSql + " WHERE (t.UserId = @UserId OR t.AssignedToUserId = @UserId)";
            var p = new DynamicParameters();
            p.Add("UserId", userId);

            AppendFilters(ref sql, p, from, to, taskTypeId, categoryId, status, search);
            sql += " ORDER BY ISNULL(tlog.LastLogDate, t.TaskDate) DESC, t.CreatedAt DESC";

            return await conn.QueryAsync<DailyTaskLog>(sql, p);
        }

        public async Task<IEnumerable<DailyTaskLog>> GetAssignedTasksAsync(int userId, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, string? search)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            // Uses user-scoped time log aggregation so agent sees only their own logged time
            var sql = @"
            SELECT t.Id, t.UserId, t.AssignedToUserId, t.TaskDate, t.TaskTypeId, t.TaskCategoryId,
                   t.TaskTitle, t.Description, t.TicketId, t.ProjectModuleId,
                   t.TimeSpentMinutes, t.Status, t.CreatedAt, t.UpdatedAt,
                   u.FullName AS UserName,
                   au.FullName AS AssignedToUserName,
                   tt.Name AS TaskTypeName,
                   tc.Name AS TaskCategoryName,
                   tk.TicketNumber, tk.Subject AS TicketSubject,
                   pm.Name AS ProjectModuleName,
                   ISNULL(tlog.TotalTime, 0) AS TotalTimeSpentMinutes,
                   tlog.LastLogDate,
                   ISNULL(tlog.LogCount, 0) AS TimeLogCount
            FROM DailyTaskLog t
            INNER JOIN UserMaster u ON u.Id = t.UserId
            LEFT JOIN UserMaster au ON au.Id = t.AssignedToUserId
            INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
            INNER JOIN TaskCategoryMaster tc ON tc.Id = t.TaskCategoryId
            LEFT JOIN HelpDeskTicket tk ON tk.Id = t.TicketId
            LEFT JOIN ProjectModuleMaster pm ON pm.Id = t.ProjectModuleId
            OUTER APPLY (
                SELECT SUM(TimeSpentMinutes) AS TotalTime,
                       MAX(LogDate) AS LastLogDate,
                       COUNT(*) AS LogCount
                FROM TaskTimeLog WHERE TaskId = t.Id AND UserId = @UserId
            ) tlog
            WHERE t.AssignedToUserId = @UserId";
            var p = new DynamicParameters();
            p.Add("UserId", userId);

            AppendFilters(ref sql, p, from, to, taskTypeId, categoryId, status, search);
            sql += " ORDER BY ISNULL(tlog.LastLogDate, t.TaskDate) DESC, t.CreatedAt DESC";

            return await conn.QueryAsync<DailyTaskLog>(sql, p);
        }

        public async Task<IEnumerable<DailyTaskLog>> GetTeamTasksAsync(IEnumerable<int> userIds, DateTime? from, DateTime? to,
            int? taskTypeId, int? categoryId, string? status, int? userFilter, string? search)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();

            var ids = userIds.ToList();
            var sql = BaseSelectSql + " WHERE t.UserId IN @UserIds";
            var p = new DynamicParameters();
            p.Add("UserIds", ids);

            if (userFilter.HasValue)
            {
                sql += " AND t.UserId = @UserFilter";
                p.Add("UserFilter", userFilter.Value);
            }

            AppendFilters(ref sql, p, from, to, taskTypeId, categoryId, status, search);
            sql += " ORDER BY ISNULL(tlog.LastLogDate, t.TaskDate) DESC, t.CreatedAt DESC";

            return await conn.QueryAsync<DailyTaskLog>(sql, p);
        }

        public async Task<DailyTaskLog?> GetByIdAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QuerySingleOrDefaultAsync<DailyTaskLog>(
                BaseSelectSql + " WHERE t.Id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(DailyTaskLog task)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            const string sql = @"
                INSERT INTO DailyTaskLog (UserId, AssignedToUserId, TaskDate, TaskTypeId, TaskCategoryId, TaskTitle,
                    Description, TicketId, ProjectModuleId, TimeSpentMinutes, Status, CreatedAt)
                VALUES (@UserId, @AssignedToUserId, @TaskDate, @TaskTypeId, @TaskCategoryId, @TaskTitle,
                    @Description, @TicketId, @ProjectModuleId, @TimeSpentMinutes, @Status, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                task.UserId, task.AssignedToUserId, task.TaskDate, task.TaskTypeId, task.TaskCategoryId,
                task.TaskTitle, task.Description, task.TicketId, task.ProjectModuleId,
                task.TimeSpentMinutes, task.Status
            });
        }

        public async Task<bool> UpdateAsync(DailyTaskLog task)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            const string sql = @"
                UPDATE DailyTaskLog SET
                    AssignedToUserId = @AssignedToUserId,
                    TaskDate = @TaskDate, TaskTypeId = @TaskTypeId, TaskCategoryId = @TaskCategoryId,
                    TaskTitle = @TaskTitle, Description = @Description, TicketId = @TicketId,
                    ProjectModuleId = @ProjectModuleId, Status = @Status, UpdatedAt = GETDATE()
                WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, new
            {
                task.Id, task.AssignedToUserId, task.TaskDate, task.TaskTypeId, task.TaskCategoryId,
                task.TaskTitle, task.Description, task.TicketId, task.ProjectModuleId,
                task.Status
            }) > 0;
        }

        public async Task<bool> UpdateTaskStatusAsync(int id, string status)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            const string sql = "UPDATE DailyTaskLog SET Status = @Status, UpdatedAt = GETDATE() WHERE Id = @Id";
            return await conn.ExecuteAsync(sql, new { Id = id, Status = status }) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.ExecuteAsync("DELETE FROM DailyTaskLog WHERE Id = @Id", new { Id = id }) > 0;
        }

        // ── Time Log CRUD ──

        public async Task<IEnumerable<TaskTimeLog>> GetTimeLogsAsync(int taskId)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<TaskTimeLog>(
                @"SELECT tl.Id, tl.TaskId, tl.UserId, tl.LogDate, tl.TimeSpentMinutes, tl.Remarks, tl.CreatedAt,
                         u.FullName AS UserName
                  FROM TaskTimeLog tl
                  INNER JOIN UserMaster u ON u.Id = tl.UserId
                  WHERE tl.TaskId = @TaskId
                  ORDER BY tl.LogDate DESC, tl.CreatedAt DESC",
                new { TaskId = taskId });
        }

        public async Task<TaskTimeLog?> GetTimeLogByIdAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QuerySingleOrDefaultAsync<TaskTimeLog>(
                @"SELECT tl.Id, tl.TaskId, tl.UserId, tl.LogDate, tl.TimeSpentMinutes, tl.Remarks, tl.CreatedAt,
                         u.FullName AS UserName
                  FROM TaskTimeLog tl
                  INNER JOIN UserMaster u ON u.Id = tl.UserId
                  WHERE tl.Id = @Id",
                new { Id = id });
        }

        public async Task<int> AddTimeLogAsync(TaskTimeLog entry)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO TaskTimeLog (TaskId, UserId, LogDate, TimeSpentMinutes, Remarks, CreatedAt)
                  VALUES (@TaskId, @UserId, @LogDate, @TimeSpentMinutes, @Remarks, GETDATE());
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { entry.TaskId, entry.UserId, entry.LogDate, entry.TimeSpentMinutes, entry.Remarks });
        }

        public async Task<bool> DeleteTimeLogAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.ExecuteAsync("DELETE FROM TaskTimeLog WHERE Id = @Id", new { Id = id }) > 0;
        }

        // ── Masters ──

        public async Task<IEnumerable<TaskTypeMaster>> GetTaskTypesAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<TaskTypeMaster>("SELECT Id, Name, IsActive FROM TaskTypeMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<IEnumerable<TaskCategoryMaster>> GetTaskCategoriesAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<TaskCategoryMaster>("SELECT Id, Name, IsActive FROM TaskCategoryMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<IEnumerable<ProjectModuleMaster>> GetProjectsAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<ProjectModuleMaster>(
                "SELECT Id, Name, Description, IsActive FROM ProjectModuleMaster WHERE IsActive = 1 ORDER BY Name");
        }

        public async Task<ProjectModuleMaster?> GetProjectByIdAsync(int id)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QuerySingleOrDefaultAsync<ProjectModuleMaster>(
                "SELECT Id, Name, Description, IsActive FROM ProjectModuleMaster WHERE Id = @Id", new { Id = id });
        }

        public async Task<int> CreateProjectAsync(ProjectModuleMaster project)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO ProjectModuleMaster (Name, Description) VALUES (@Name, @Description);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new { project.Name, project.Description });
        }

        public async Task<bool> UpdateProjectAsync(ProjectModuleMaster project)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.ExecuteAsync(
                "UPDATE ProjectModuleMaster SET Name = @Name, Description = @Description, IsActive = @IsActive WHERE Id = @Id",
                new { project.Id, project.Name, project.Description, project.IsActive }) > 0;
        }

        // ── Ticket Search ──

        public async Task<IEnumerable<(int Id, string TicketNumber, string Subject)>> SearchTicketsAsync(string term)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var rows = await conn.QueryAsync<(int Id, string TicketNumber, string Subject)>(
                @"SELECT TOP 15 Id, TicketNumber, Subject
                  FROM HelpDeskTicket
                  WHERE TicketNumber LIKE @Term OR Subject LIKE @Term
                  ORDER BY Id DESC",
                new { Term = $"%{term}%" });
            return rows;
        }

        // ── Assignable Users (Ticket Admin + Ticket Agent) ──

        public async Task<IEnumerable<UserMaster>> GetAssignableUsersAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            return await conn.QueryAsync<UserMaster>(
                @"SELECT DISTINCT u.Id, u.FullName, u.Username, u.Email, u.IsActive
                  FROM UserMaster u
                  INNER JOIN UserRoleMap ur ON ur.UserId = u.Id
                  INNER JOIN RoleMaster r ON r.Id = ur.RoleId
                  WHERE u.IsActive = 1 AND r.RoleName IN ('Administrator','Ticket Admin','Ticket Agent')
                  ORDER BY u.FullName");
        }

        // ── Summary ──

        public async Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
            int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetSummaryAsync(int userId, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var sql = @"
                SELECT COUNT(*) AS TotalTasks,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS TotalMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Development' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS DevMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Support' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS SupportMinutes,
                       ISNULL(SUM(CASE WHEN t.Status = 'Pending' THEN 1 ELSE 0 END), 0) AS PendingCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'In Progress' THEN 1 ELSE 0 END), 0) AS InProgressCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END), 0) AS CompletedCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Cancelled' THEN 1 ELSE 0 END), 0) AS CancelledCount
                FROM DailyTaskLog t
                INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
                OUTER APPLY (
                    SELECT SUM(TimeSpentMinutes) AS TotalTime FROM TaskTimeLog WHERE TaskId = t.Id
                ) tlog
                WHERE t.UserId = @UserId";
            var p = new DynamicParameters();
            p.Add("UserId", userId);
            if (from.HasValue) { sql += " AND t.TaskDate >= @From"; p.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND t.TaskDate <= @To"; p.Add("To", to.Value); }

            var row = await conn.QuerySingleAsync<dynamic>(sql, p);
            return ((int)row.TotalTasks, (int)row.TotalMinutes, (int)row.DevMinutes, (int)row.SupportMinutes,
                    (int)row.PendingCount, (int)row.InProgressCount, (int)row.CompletedCount, (int)row.CancelledCount);
        }

        public async Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
            int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetAssignedSummaryAsync(int userId, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var sql = @"
                SELECT COUNT(*) AS TotalTasks,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS TotalMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Development' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS DevMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Support' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS SupportMinutes,
                       ISNULL(SUM(CASE WHEN t.Status = 'Pending' THEN 1 ELSE 0 END), 0) AS PendingCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'In Progress' THEN 1 ELSE 0 END), 0) AS InProgressCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END), 0) AS CompletedCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Cancelled' THEN 1 ELSE 0 END), 0) AS CancelledCount
                FROM DailyTaskLog t
                INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
                OUTER APPLY (
                    SELECT SUM(TimeSpentMinutes) AS TotalTime FROM TaskTimeLog WHERE TaskId = t.Id AND UserId = @UserId
                ) tlog
                WHERE t.AssignedToUserId = @UserId";
            var p = new DynamicParameters();
            p.Add("UserId", userId);
            if (from.HasValue) { sql += " AND t.TaskDate >= @From"; p.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND t.TaskDate <= @To"; p.Add("To", to.Value); }

            var row = await conn.QuerySingleAsync<dynamic>(sql, p);
            return ((int)row.TotalTasks, (int)row.TotalMinutes, (int)row.DevMinutes, (int)row.SupportMinutes,
                    (int)row.PendingCount, (int)row.InProgressCount, (int)row.CompletedCount, (int)row.CancelledCount);
        }

        public async Task<(int TotalTasks, int TotalMinutes, int DevMinutes, int SupportMinutes,
            int PendingCount, int InProgressCount, int CompletedCount, int CancelledCount)>
            GetTeamSummaryAsync(IEnumerable<int> userIds, DateTime? from, DateTime? to)
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var ids = userIds.ToList();
            var sql = @"
                SELECT COUNT(*) AS TotalTasks,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS TotalMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Development' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS DevMinutes,
                       ISNULL(SUM(CASE WHEN t.Status <> 'Cancelled' AND tt.Name = 'Support' THEN ISNULL(tlog.TotalTime, 0) ELSE 0 END), 0) AS SupportMinutes,
                       ISNULL(SUM(CASE WHEN t.Status = 'Pending' THEN 1 ELSE 0 END), 0) AS PendingCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'In Progress' THEN 1 ELSE 0 END), 0) AS InProgressCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END), 0) AS CompletedCount,
                       ISNULL(SUM(CASE WHEN t.Status = 'Cancelled' THEN 1 ELSE 0 END), 0) AS CancelledCount
                FROM DailyTaskLog t
                INNER JOIN TaskTypeMaster tt ON tt.Id = t.TaskTypeId
                OUTER APPLY (
                    SELECT SUM(TimeSpentMinutes) AS TotalTime FROM TaskTimeLog WHERE TaskId = t.Id
                ) tlog
                WHERE t.UserId IN @UserIds";
            var p = new DynamicParameters();
            p.Add("UserIds", ids);
            if (from.HasValue) { sql += " AND t.TaskDate >= @From"; p.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND t.TaskDate <= @To"; p.Add("To", to.Value); }

            var row = await conn.QuerySingleAsync<dynamic>(sql, p);
            return ((int)row.TotalTasks, (int)row.TotalMinutes, (int)row.DevMinutes, (int)row.SupportMinutes,
                    (int)row.PendingCount, (int)row.InProgressCount, (int)row.CompletedCount, (int)row.CancelledCount);
        }

        // ── Table Bootstrapping ──

        public async Task EnsureTablesAsync()
        {
            using var conn = CreateConnection();
            await ((SqlConnection)conn).OpenAsync();
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SqlScripts", "062_CreateDailyTaskLogTables.sql");
            if (!File.Exists(scriptPath)) return;
            var sql = await File.ReadAllTextAsync(scriptPath);
            foreach (var batch in sql.Split(new[] { "\nGO\n", "\nGO\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(batch))
                    await conn.ExecuteAsync(batch);
            }
        }

        // ── Private Helpers ──

        private static void AppendFilters(ref string sql, DynamicParameters p,
            DateTime? from, DateTime? to, int? taskTypeId, int? categoryId, string? status, string? search)
        {
            if (from.HasValue) { sql += " AND t.TaskDate >= @From"; p.Add("From", from.Value); }
            if (to.HasValue) { sql += " AND t.TaskDate <= @To"; p.Add("To", to.Value); }
            if (taskTypeId.HasValue) { sql += " AND t.TaskTypeId = @TaskTypeId"; p.Add("TaskTypeId", taskTypeId.Value); }
            if (categoryId.HasValue) { sql += " AND t.TaskCategoryId = @CategoryId"; p.Add("CategoryId", categoryId.Value); }
            if (!string.IsNullOrWhiteSpace(status)) { sql += " AND t.Status = @Status"; p.Add("Status", status); }
            if (!string.IsNullOrWhiteSpace(search))
            {
                sql += " AND (t.TaskTitle LIKE @Search OR t.Description LIKE @Search)";
                p.Add("Search", $"%{search}%");
            }
        }
    }
}

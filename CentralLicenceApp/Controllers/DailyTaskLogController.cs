using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin,Ticket Agent")]
    public class DailyTaskLogController : Controller
    {
        private readonly IDailyTaskLogRepository _taskRepo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<DailyTaskLogController> _logger;

        public DailyTaskLogController(
            IDailyTaskLogRepository taskRepo,
            IUserRepository userRepo,
            ILogger<DailyTaskLogController> logger)
        {
            _taskRepo = taskRepo;
            _userRepo = userRepo;
            _logger   = logger;
        }

        private bool IsAdminOrTicketAdmin =>
            User.IsInRole("Administrator") || User.IsInRole("Ticket Admin");

        // ── My Tasks ──
        public async Task<IActionResult> Index(DateTime? from, DateTime? to,
            int? taskType, int? category, string? status, string? search)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Challenge();

            var isAgent = !IsAdminOrTicketAdmin;
            var tasks = isAgent
                ? (await _taskRepo.GetAssignedTasksAsync(userId, from, to, taskType, category, status, search)).ToList()
                : (await _taskRepo.GetTasksAsync(userId, from, to, taskType, category, status, search)).ToList();
            var summary = isAgent
                ? await _taskRepo.GetAssignedSummaryAsync(userId, from, to)
                : await _taskRepo.GetSummaryAsync(userId, from, to);
            var taskTypes = (await _taskRepo.GetTaskTypesAsync()).ToList();
            var categories = (await _taskRepo.GetTaskCategoriesAsync()).ToList();

            var vm = new DailyTaskLogIndexViewModel
            {
                Tasks = tasks,
                FromDate = from,
                ToDate = to,
                TaskTypeFilter = taskType,
                CategoryFilter = category,
                StatusFilter = status,
                SearchTerm = search,
                TaskTypes = taskTypes,
                TaskCategories = categories,
                TotalTasks = summary.TotalTasks,
                TotalMinutes = summary.TotalMinutes,
                DevMinutes = summary.DevMinutes,
                SupportMinutes = summary.SupportMinutes,
                PendingCount = summary.PendingCount,
                InProgressCount = summary.InProgressCount,
                CompletedCount = summary.CompletedCount,
                CancelledCount = summary.CancelledCount,
                IsManagerView = false,
                CanCreateTask = IsAdminOrTicketAdmin,
                CanEditDelete = IsAdminOrTicketAdmin
            };

            return View(vm);
        }

        // ── Team View ──
        public async Task<IActionResult> TeamView(DateTime? from, DateTime? to,
            int? taskType, int? category, string? status, int? user, string? search)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();

            var isAdmin = IsAdminOrTicketAdmin;
            if (!isAdmin && !currentUser.IsCoreMember)
                return RedirectToAction("Index");

            List<int> teamIds;
            List<UserMaster> teamMembers;
            if (isAdmin)
            {
                teamMembers = (await _userRepo.GetAllAsync()).Where(u => u.IsActive).ToList();
                teamIds = teamMembers.Select(u => u.Id).ToList();
            }
            else
            {
                var subIds = await _userRepo.GetSelfAndSubordinateIdsAsync(currentUser.Id);
                teamIds = subIds.ToList();
                teamMembers = new List<UserMaster>();
                foreach (var id in teamIds)
                {
                    var u = await _userRepo.GetByIdAsync(id);
                    if (u != null) teamMembers.Add(u);
                }
            }

            var tasks = (await _taskRepo.GetTeamTasksAsync(teamIds, from, to, taskType, category, status, user, search)).ToList();
            var summary = await _taskRepo.GetTeamSummaryAsync(teamIds, from, to);
            var taskTypes = (await _taskRepo.GetTaskTypesAsync()).ToList();
            var categories = (await _taskRepo.GetTaskCategoriesAsync()).ToList();

            var vm = new DailyTaskLogIndexViewModel
            {
                Tasks = tasks,
                FromDate = from,
                ToDate = to,
                TaskTypeFilter = taskType,
                CategoryFilter = category,
                StatusFilter = status,
                UserFilter = user,
                SearchTerm = search,
                TaskTypes = taskTypes,
                TaskCategories = categories,
                TeamMembers = teamMembers,
                TotalTasks = summary.TotalTasks,
                TotalMinutes = summary.TotalMinutes,
                DevMinutes = summary.DevMinutes,
                SupportMinutes = summary.SupportMinutes,
                PendingCount = summary.PendingCount,
                InProgressCount = summary.InProgressCount,
                CompletedCount = summary.CompletedCount,
                CancelledCount = summary.CancelledCount,
                IsManagerView = true,
                CanCreateTask = IsAdminOrTicketAdmin,
                CanEditDelete = IsAdminOrTicketAdmin
            };

            return View("Index", vm);
        }

        // ── Create Task (Admin / Ticket Admin only) ──
        [HttpGet]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = await BuildFormViewModelAsync(new DailyTaskLog { TaskDate = DateTime.Today });
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Create(DailyTaskLogFormViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Challenge();

            if (string.IsNullOrWhiteSpace(model.Task.TaskTitle) || model.Task.TaskTypeId == 0 || model.Task.TaskCategoryId == 0)
            {
                TempData["Error"] = "Please fill all required fields (Title, Task Type, Category).";
                var vm = await BuildFormViewModelAsync(model.Task);
                return View(vm);
            }

            model.Task.UserId = userId;

            var taskId = await _taskRepo.CreateAsync(model.Task);

            // Create initial time log if time was provided
            if (model.Task.TimeSpentMinutes > 0)
            {
                await _taskRepo.AddTimeLogAsync(new TaskTimeLog
                {
                    TaskId = taskId,
                    UserId = userId,
                    LogDate = model.Task.TaskDate,
                    TimeSpentMinutes = model.Task.TimeSpentMinutes,
                    Remarks = "Initial time entry"
                });
            }

            TempData["Success"] = "Task created successfully.";
            return RedirectToAction("Index");
        }

        // ── View Task Details ──
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null) return NotFound();

            var userId = GetCurrentUserId();

            // Non-admin roles can only view tasks they own or are assigned to
            if (!IsAdminOrTicketAdmin)
            {
                if (task.UserId != userId && task.AssignedToUserId != userId)
                    return Forbid();
            }

            var timeLogs = (await _taskRepo.GetTimeLogsAsync(id)).ToList();
            ViewBag.TimeLogs = timeLogs;
            ViewBag.CanEditDelete = IsAdminOrTicketAdmin;
            return View(task);
        }

        // ── Edit Task (Admin / Ticket Admin only) ──
        [HttpGet]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null) return NotFound();

            var userId = GetCurrentUserId();
            if (task.UserId != userId && !IsAdminOrTicketAdmin)
                return Forbid();

            var vm = await BuildFormViewModelAsync(task, isEdit: true);
            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Edit(int id, DailyTaskLogFormViewModel model)
        {
            var existing = await _taskRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            var userId = GetCurrentUserId();
            if (existing.UserId != userId && !IsAdminOrTicketAdmin)
                return Forbid();

            if (string.IsNullOrWhiteSpace(model.Task.TaskTitle) || model.Task.TaskTypeId == 0 || model.Task.TaskCategoryId == 0)
            {
                TempData["Error"] = "Please fill all required fields (Title, Task Type, Category).";
                model.Task.Id = id;
                var vm = await BuildFormViewModelAsync(model.Task, isEdit: true);
                return View("Create", vm);
            }

            model.Task.Id = id;
            model.Task.UserId = existing.UserId;

            await _taskRepo.UpdateAsync(model.Task);
            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction("Index");
        }

        // ── Delete Task (Admin / Ticket Admin only, AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _taskRepo.GetByIdAsync(id);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            var userId = GetCurrentUserId();
            // Only the creator can delete
            if (task.UserId != userId)
                return Json(new { success = false, message = "Only the task creator can delete this task." });

            // Cannot delete if time has been logged
            var timeLogs = (await _taskRepo.GetTimeLogsAsync(id)).ToList();
            if (timeLogs.Any(tl => tl.TimeSpentMinutes > 0))
                return Json(new { success = false, message = "This task has logged time and cannot be deleted. Use 'Cancel Task' instead." });

            await _taskRepo.DeleteAsync(id);
            return Json(new { success = true, message = "Task deleted successfully." });
        }

        // ── Cancel Task (Admin / Ticket Admin only, AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> CancelTask(int taskId, string? note)
        {
            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.Status == "Cancelled")
                return Json(new { success = false, message = "Task is already cancelled." });

            if (string.IsNullOrWhiteSpace(note))
                return Json(new { success = false, message = "Please provide a reason for cancellation." });

            var userId = GetCurrentUserId();
            await _taskRepo.UpdateTaskStatusAsync(taskId, "Cancelled");

            // Log the cancellation as a 0-minute time entry
            await _taskRepo.AddTimeLogAsync(new TaskTimeLog
            {
                TaskId = taskId,
                UserId = userId,
                LogDate = DateTime.Today,
                TimeSpentMinutes = 0,
                Remarks = $"Task Cancelled: {note.Trim()}"
            });

            return Json(new { success = true, message = "Task cancelled successfully. Logged time for this task is excluded from totals." });
        }

        // ── Log Time (AJAX) — All roles can log time ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogTime(int taskId, DateTime logDate, int hours, int minutes, string? remarks)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false, message = "Not authenticated." });

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.Status == "Completed")
                return Json(new { success = false, message = "Cannot log time on a completed task." });

            // Ticket Agent can only log time on tasks assigned to them or created by them
            if (!IsAdminOrTicketAdmin)
            {
                if (task.UserId != userId && task.AssignedToUserId != userId)
                    return Json(new { success = false, message = "Not authorized." });
            }

            var totalMinutes = hours * 60 + minutes;
            if (totalMinutes <= 0)
                return Json(new { success = false, message = "Time must be greater than 0." });

            var entry = new TaskTimeLog
            {
                TaskId = taskId,
                UserId = userId,
                LogDate = logDate,
                TimeSpentMinutes = totalMinutes,
                Remarks = remarks?.Trim()
            };

            await _taskRepo.AddTimeLogAsync(entry);
            return Json(new { success = true, message = "Time logged successfully." });
        }

        // ── Change Status (AJAX) — All roles can change status on their tasks ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int taskId, string status, string? note)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Json(new { success = false, message = "Not authenticated." });

            var validStatuses = new[] { "Pending", "Started", "In Progress", "On Hold", "Completed", "Cancelled" };
            if (string.IsNullOrWhiteSpace(status) || !validStatuses.Contains(status))
                return Json(new { success = false, message = "Invalid status." });

            if (string.IsNullOrWhiteSpace(note))
                return Json(new { success = false, message = "Please provide a note / reason for the status change." });

            var task = await _taskRepo.GetByIdAsync(taskId);
            if (task == null) return Json(new { success = false, message = "Task not found." });

            // Ticket Agent can only change status on tasks assigned to them
            if (!IsAdminOrTicketAdmin)
            {
                if (task.AssignedToUserId != userId)
                    return Json(new { success = false, message = "Not authorized." });
            }

            await _taskRepo.UpdateTaskStatusAsync(taskId, status);

            // Log the status change as a time log entry with 0 minutes
            await _taskRepo.AddTimeLogAsync(new TaskTimeLog
            {
                TaskId = taskId,
                UserId = userId,
                LogDate = DateTime.Today,
                TimeSpentMinutes = 0,
                Remarks = $"Status changed to '{status}': {note.Trim()}"
            });

            return Json(new { success = true, message = "Status updated to '" + status + "'." });
        }

        // ── Get Time Logs (AJAX) ──
        [HttpGet]
        public async Task<IActionResult> GetTimeLogs(int taskId)
        {
            var logs = await _taskRepo.GetTimeLogsAsync(taskId);
            return Json(logs.Select(l => new
            {
                l.Id,
                l.TaskId,
                logDate = l.LogDate.ToString("dd MMM yyyy"),
                l.TimeSpentMinutes,
                hours = l.TimeSpentMinutes / 60,
                mins = l.TimeSpentMinutes % 60,
                l.Remarks,
                l.UserName
            }));
        }

        // ── Delete Time Log (Admin / Ticket Admin only, AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> DeleteTimeLog(int id)
        {
            var log = await _taskRepo.GetTimeLogByIdAsync(id);
            if (log == null) return Json(new { success = false, message = "Time log not found." });

            var userId = GetCurrentUserId();
            if (log.UserId != userId && !IsAdminOrTicketAdmin)
                return Json(new { success = false, message = "Not authorized." });

            await _taskRepo.DeleteTimeLogAsync(id);
            return Json(new { success = true, message = "Time log deleted." });
        }

        // ── Ticket Search API ──
        [HttpGet]
        public async Task<IActionResult> SearchTickets(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(Array.Empty<object>());

            var results = await _taskRepo.SearchTicketsAsync(term);
            return Json(results.Select(r => new { r.Id, r.TicketNumber, r.Subject }));
        }

        // ── Quick-Add Project (Admin / Ticket Admin only, AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> CreateProject(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, message = "Name is required." });

            var id = await _taskRepo.CreateProjectAsync(new ProjectModuleMaster { Name = name.Trim(), Description = description?.Trim() });
            return Json(new { success = true, id, name = name.Trim() });
        }

        // ── Helpers ──

        private int GetCurrentUserId()
        {
            var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(val, out var id) ? id : 0;
        }

        private async Task<UserMaster?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            return userId == 0 ? null : await _userRepo.GetByIdAsync(userId);
        }

        private async Task<DailyTaskLogFormViewModel> BuildFormViewModelAsync(DailyTaskLog task, bool isEdit = false)
        {
            return new DailyTaskLogFormViewModel
            {
                Task = task,
                TaskTypes = (await _taskRepo.GetTaskTypesAsync()).ToList(),
                TaskCategories = (await _taskRepo.GetTaskCategoriesAsync()).ToList(),
                Projects = (await _taskRepo.GetProjectsAsync()).ToList(),
                AssignableUsers = (await _taskRepo.GetAssignableUsersAsync()).ToList(),
                IsEdit = isEdit
            };
        }
    }
}

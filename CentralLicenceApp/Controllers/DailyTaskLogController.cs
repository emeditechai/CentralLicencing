using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
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
        private readonly IWebHostEnvironment _env;
        private readonly ITaskEmailService _taskEmailService;

        private static readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB per file

        public DailyTaskLogController(
            IDailyTaskLogRepository taskRepo,
            IUserRepository userRepo,
            ILogger<DailyTaskLogController> logger,
            IWebHostEnvironment env,
            ITaskEmailService taskEmailService)
        {
            _taskRepo         = taskRepo;
            _userRepo         = userRepo;
            _logger           = logger;
            _env              = env;
            _taskEmailService = taskEmailService;
        }

        private bool IsAdminOrTicketAdmin =>
            User.IsInRole("Administrator") || User.IsInRole("Ticket Admin");

        // ── My Tasks ──
        public async Task<IActionResult> Index(DateTime? from, DateTime? to,
            int? taskType, int? category, string? status, string? search)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();
            var userId = currentUser.Id;

            var isAgent = !IsAdminOrTicketAdmin;
            var tasks = isAgent
                ? (await _taskRepo.GetAssignedTasksAsync(userId, currentUser.FullName ?? "", from, to, taskType, category, status, search)).ToList()
                : (await _taskRepo.GetTasksAsync(userId, currentUser.FullName ?? "", from, to, taskType, category, status, search)).ToList();
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

            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserFullName = currentUser.FullName;
            ViewBag.IsCoreMember = currentUser.IsCoreMember;

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

            var tasks = (await _taskRepo.GetTeamTasksAsync(teamIds, currentUser.FullName ?? "", from, to, taskType, category, status, user, search)).ToList();
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

            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserFullName = currentUser.FullName;
            ViewBag.IsCoreMember = currentUser.IsCoreMember;

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

            // Save attachments
            await SaveAttachmentsAsync(model.Attachments, taskId, userId);

            // Email notification — fire after save, do NOT await to keep request fast
            if (model.Task.AssignedToUserId.HasValue)
            {
                var fullTask = await _taskRepo.GetByIdAsync(taskId);
                if (fullTask != null && !string.IsNullOrWhiteSpace(fullTask.AssignedToUserName))
                {
                    var assignee = await _userRepo.GetByIdAsync(model.Task.AssignedToUserId.Value);
                    if (assignee != null && !string.IsNullOrWhiteSpace(assignee.Email))
                    {
                        _ = Task.Run(() => _taskEmailService.NotifyTaskAssignedAsync(fullTask, assignee.FullName ?? assignee.Username, assignee.Email));
                    }
                }
            }

            TempData["Success"] = "Task created successfully.";
            return RedirectToAction("Details", new { id = taskId });
        }

        // ── View Task Details ──
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(id, currentUser.FullName ?? "");
            if (task == null) return NotFound();

            // Non-admin/core roles can only view tasks they own or are assigned to
            if (!IsAdminOrTicketAdmin && !currentUser.IsCoreMember)
            {
                if (task.UserId != userId && task.AssignedToUserId != userId)
                    return Forbid();
            }

            var timeLogs = (await _taskRepo.GetTimeLogsAsync(id)).ToList();
            var attachments = (await _taskRepo.GetAttachmentsAsync(id)).ToList();
            var comments = (await _taskRepo.GetCommentsAsync(id)).ToList();

            ViewBag.TimeLogs = timeLogs;
            ViewBag.Attachments = attachments;
            ViewBag.Comments = comments;
            ViewBag.CurrentUserId = currentUser.Id;
            ViewBag.CurrentUserFullName = currentUser.FullName;
            ViewBag.IsCoreMember = currentUser.IsCoreMember;
            ViewBag.AssignableUsers = await _taskRepo.GetAssignableUsersAsync();
            return View(task);
        }

        // ── Comments ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int TaskId, string CommentText)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(TaskId, currentUser.FullName ?? "");
            if (task == null) return NotFound();

            if (!IsAdminOrTicketAdmin && !currentUser.IsCoreMember)
            {
                if (task.UserId != userId && task.AssignedToUserId != userId)
                    return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(CommentText))
            {
                var comment = new TaskComment
                {
                    TaskId = TaskId,
                    UserId = userId,
                    CommentText = CommentText
                };
                await _taskRepo.AddCommentAsync(comment);

                // Handle Mentions
                var assignableUsers = await _taskRepo.GetAssignableUsersAsync();
                var taggedUsers = new List<UserMaster>();
                foreach (var u in assignableUsers)
                {
                    if (CommentText.Contains($"@{u.FullName}", StringComparison.OrdinalIgnoreCase))
                    {
                        taggedUsers.Add(u);
                    }
                }

                if (taggedUsers.Any())
                {
                    var senderName = currentUser?.FullName ?? "Someone";
                    foreach (var taggedUser in taggedUsers)
                    {
                        if (!string.IsNullOrEmpty(taggedUser.Email))
                        {
                            _ = Task.Run(() => _taskEmailService.NotifyTaskCommentMentionAsync(task, senderName, taggedUser.FullName ?? "", taggedUser.Email, CommentText));
                        }
                    }
                }

                TempData["Success"] = "Comment added successfully.";
            }
            return RedirectToAction("Details", new { id = TaskId });
        }

        // ── Edit Task ──
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(id, currentUser.FullName ?? "");
            if (task == null) return NotFound();

            if (task.UserId != userId && task.AssignedToUserId != userId && !currentUser.IsCoreMember)
                return Forbid();

            var vm = await BuildFormViewModelAsync(task, isEdit: true);
            vm.ExistingAttachments = (await _taskRepo.GetAttachmentsAsync(id)).ToList();
            return View("Create", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DailyTaskLogFormViewModel model)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();
            var userId = currentUser.Id;

            var existing = await _taskRepo.GetByIdAsync(id, currentUser.FullName ?? "");
            if (existing == null) return NotFound();

            if (existing.UserId != userId && existing.AssignedToUserId != userId && !currentUser.IsCoreMember)
                return Forbid();

            if (string.IsNullOrWhiteSpace(model.Task.TaskTitle) || model.Task.TaskTypeId == 0 || model.Task.TaskCategoryId == 0)
            {
                TempData["Error"] = "Please fill all required fields (Title, Task Type, Category).";
                model.Task.Id = id;
                var vm = await BuildFormViewModelAsync(model.Task, isEdit: true);
                vm.ExistingAttachments = (await _taskRepo.GetAttachmentsAsync(id)).ToList();
                return View("Create", vm);
            }

            model.Task.Id = id;
            model.Task.UserId = existing.UserId;

            // ── Detect changes before persisting ──
            var changes = new List<string>();

            if (!string.Equals(existing.TaskTitle, model.Task.TaskTitle, StringComparison.Ordinal))
                changes.Add($"Title: \"{existing.TaskTitle}\" → \"{model.Task.TaskTitle}\"");

            if (existing.TaskDate.Date != model.Task.TaskDate.Date)
                changes.Add($"Task Date: {existing.TaskDate:dd MMM yyyy} → {model.Task.TaskDate:dd MMM yyyy}");

            if (existing.TaskTypeId != model.Task.TaskTypeId)
                changes.Add($"Task Type changed");

            if (existing.TaskCategoryId != model.Task.TaskCategoryId)
                changes.Add($"Category changed");

            if (!string.Equals(existing.Description?.Trim(), model.Task.Description?.Trim(), StringComparison.Ordinal))
                changes.Add("Description updated");

            if (existing.AssignedToUserId != model.Task.AssignedToUserId)
            {
                var oldName = string.IsNullOrWhiteSpace(existing.AssignedToUserName) ? "Unassigned" : existing.AssignedToUserName;
                var newUser = model.Task.AssignedToUserId.HasValue
                    ? (await _userRepo.GetByIdAsync(model.Task.AssignedToUserId.Value))?.FullName ?? "Unknown"
                    : "Unassigned";
                changes.Add($"Assigned To: \"{oldName}\" → \"{newUser}\"");
            }

            if (!string.Equals(existing.Status, model.Task.Status, StringComparison.Ordinal))
                changes.Add($"Status: \"{existing.Status}\" → \"{model.Task.Status}\"");

            if (existing.ProjectModuleId != model.Task.ProjectModuleId)
                changes.Add("Project / Module changed");

            if (existing.TicketId != model.Task.TicketId)
                changes.Add("Linked Ticket changed");

            await _taskRepo.UpdateAsync(model.Task);

            // Save any newly-uploaded attachments
            await SaveAttachmentsAsync(model.Attachments, id, userId);

            // Email notification — only fire when something actually changed AND assignee has an email
            if (changes.Count > 0 && model.Task.AssignedToUserId.HasValue)
            {
                var fullTask = await _taskRepo.GetByIdAsync(id);
                if (fullTask != null)
                {
                    var assignee = await _userRepo.GetByIdAsync(model.Task.AssignedToUserId.Value);
                    if (assignee != null && !string.IsNullOrWhiteSpace(assignee.Email))
                    {
                        var capturedChanges = changes.ToList(); // capture for the closure
                        _ = Task.Run(() => _taskEmailService.NotifyTaskUpdatedAsync(
                            fullTask, assignee.FullName ?? assignee.Username, assignee.Email, capturedChanges));
                    }
                }
            }

            TempData["Success"] = "Task updated successfully.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(id, currentUser.FullName ?? "");
            if (task == null) return Json(new { success = false, message = "Task not found." });

            // Only the creator or Core Member or Admin can delete
            if (task.UserId != userId && !currentUser.IsCoreMember)
                return Json(new { success = false, message = "Only the task creator or admin can delete this task." });

            // Cannot delete if time has been logged
            var timeLogs = (await _taskRepo.GetTimeLogsAsync(id)).ToList();
            if (timeLogs.Any(tl => tl.TimeSpentMinutes > 0))
                return Json(new { success = false, message = "This task has logged time and cannot be deleted. Use 'Cancel Task' instead." });

            // Delete attachment files from disk before deleting task
            var attachments = (await _taskRepo.GetAttachmentsAsync(id)).ToList();
            foreach (var att in attachments)
                DeletePhysicalFile(att.FilePath);

            await _taskRepo.DeleteAsync(id);
            return Json(new { success = true, message = "Task deleted successfully." });
        }

        // ── Delete Attachment (AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var att = await _taskRepo.GetAttachmentByIdAsync(id);
            if (att == null) return Json(new { success = false, message = "Attachment not found." });

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });

            var task = await _taskRepo.GetByIdAsync(att.TaskId, currentUser.FullName ?? "");
            if (task != null && task.UserId != currentUser.Id && task.AssignedToUserId != currentUser.Id && !currentUser.IsCoreMember)
                return Json(new { success = false, message = "Not authorized to delete attachments for this task." });

            DeletePhysicalFile(att.FilePath);
            await _taskRepo.DeleteAttachmentAsync(id);

            return Json(new { success = true, message = "Attachment deleted." });
        }

        // ── Cancel Task (AJAX) ──
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelTask(int taskId, string? note)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(taskId, currentUser.FullName ?? "");
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.UserId != userId && task.AssignedToUserId != userId && !currentUser.IsCoreMember)
                return Json(new { success = false, message = "Not authorized to cancel this task." });

            if (task.Status == "Cancelled")
                return Json(new { success = false, message = "Task is already cancelled." });

            if (string.IsNullOrWhiteSpace(note))
                return Json(new { success = false, message = "Please provide a reason for cancellation." });
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
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });
            var userId = currentUser.Id;

            var task = await _taskRepo.GetByIdAsync(taskId, currentUser.FullName ?? "");
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.Status == "Completed")
                return Json(new { success = false, message = "Cannot log time on a completed task." });

            if (task.UserId != userId && task.AssignedToUserId != userId && !task.IsCurrentUserTagged && !currentUser.IsCoreMember)
                return Json(new { success = false, message = "Not authorized to log time for this task." });

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
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });
            var userId = currentUser.Id;

            var validStatuses = new[] { "Pending", "Started", "In Progress", "On Hold", "Completed", "Cancelled", "Resolved" };
            if (string.IsNullOrWhiteSpace(status) || !validStatuses.Contains(status))
                return Json(new { success = false, message = "Invalid status." });

            if (string.IsNullOrWhiteSpace(note))
                return Json(new { success = false, message = "Please provide a note / reason for the status change." });

            var task = await _taskRepo.GetByIdAsync(taskId, currentUser.FullName ?? "");
            if (task == null) return Json(new { success = false, message = "Task not found." });

            if (task.UserId != userId && task.AssignedToUserId != userId && !task.IsCurrentUserTagged && !currentUser.IsCoreMember)
            {
                return Json(new { success = false, message = "Not authorized to change status on this task." });
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

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Json(new { success = false, message = "Not authenticated." });

            var task = await _taskRepo.GetByIdAsync(log.TaskId, currentUser.FullName ?? "");
            if (task != null && task.UserId != currentUser.Id && log.UserId != currentUser.Id && !currentUser.IsCoreMember)
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

        /// <summary>
        /// Validates and persists uploaded IFormFile list to disk + DB.
        /// </summary>
        private async Task SaveAttachmentsAsync(IEnumerable<IFormFile>? files, int taskId, int userId)
        {
            if (files == null) return;

            var uploadRoot = Path.Combine(_env.WebRootPath, "uploads", "task-attachments", taskId.ToString());
            Directory.CreateDirectory(uploadRoot);

            foreach (var file in files)
            {
                if (file == null || file.Length == 0) continue;

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) continue;
                if (file.Length > MaxFileSizeBytes) continue;

                var storedName = $"{Guid.NewGuid()}{ext}";
                var physicalPath = Path.Combine(uploadRoot, storedName);

                using (var fs = new FileStream(physicalPath, FileMode.Create))
                    await file.CopyToAsync(fs);

                var relPath = $"/uploads/task-attachments/{taskId}/{storedName}";

                await _taskRepo.AddAttachmentAsync(new TaskAttachment
                {
                    TaskId = taskId,
                    FileName = storedName,
                    OriginalName = file.FileName,
                    FilePath = relPath,
                    FileSize = file.Length,
                    UploadedById = userId
                });
            }
        }

        /// <summary>Deletes physical file from wwwroot using its relative web path.</summary>
        private void DeletePhysicalFile(string relPath)
        {
            try
            {
                var physical = Path.Combine(_env.WebRootPath, relPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(physical))
                    System.IO.File.Delete(physical);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete attachment file: {Path}", relPath);
            }
        }
    }
}

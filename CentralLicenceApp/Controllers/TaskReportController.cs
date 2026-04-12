using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin,Ticket Agent")]
    public class TaskReportController : Controller
    {
        private readonly ITaskReportRepository _reportRepo;
        private readonly IDailyTaskLogRepository _taskRepo;
        private readonly IUserRepository _userRepo;

        public TaskReportController(
            ITaskReportRepository reportRepo,
            IDailyTaskLogRepository taskRepo,
            IUserRepository userRepo)
        {
            _reportRepo = reportRepo;
            _taskRepo = taskRepo;
            _userRepo = userRepo;
        }

        private bool IsAdminOrTicketAdmin =>
            User.IsInRole("Administrator") || User.IsInRole("Ticket Admin");

        private async Task<(bool isAdmin, int? userId)> ResolveReportScopeAsync()
        {
            if (IsAdminOrTicketAdmin)
                return (true, null);

            var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(idValue, out var parsed))
                return (false, parsed);

            var userName = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = await _userRepo.GetByUsernameAsync(userName);
                if (user != null) return (false, user.Id);
            }
            return (false, null);
        }

        // ── 1) Timesheet Report ──
        public async Task<IActionResult> Timesheet(DateTime? fromDate, DateTime? toDate, int? user, int? taskType, int page = 1)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            var (isAdmin, scopeUserId) = await ResolveReportScopeAsync();
            int? effectiveUser = isAdmin ? user : scopeUserId;

            var (items, totalCount) = await _reportRepo.GetTimesheetReportAsync(
                fromDate, toDate, effectiveUser, taskType, page, 20);

            var taskTypes = (await _taskRepo.GetTaskTypesAsync()).ToList();
            var users = isAdmin ? (await _taskRepo.GetAssignableUsersAsync()).ToList() : new();

            var vm = new TimesheetReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                UserFilter = user,
                TaskTypeFilter = taskType,
                IsAdminView = isAdmin,
                Items = items.ToList(),
                Users = users,
                TaskTypes = taskTypes,
                PageNumber = page,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // ── 2) Employee Productivity Report ──
        public async Task<IActionResult> EmployeeProductivity(DateTime? fromDate, DateTime? toDate, int? user)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            var (isAdmin, scopeUserId) = await ResolveReportScopeAsync();
            int? effectiveUser = isAdmin ? user : scopeUserId;

            var items = await _reportRepo.GetEmployeeProductivityReportAsync(fromDate, toDate, effectiveUser);
            var users = isAdmin ? (await _taskRepo.GetAssignableUsersAsync()).ToList() : new();

            var vm = new EmployeeProductivityReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                UserFilter = user,
                IsAdminView = isAdmin,
                Items = items.ToList(),
                Users = users
            };

            return View(vm);
        }

        // ── 3) Project / Module Effort Report ──
        public async Task<IActionResult> ProjectEffort(DateTime? fromDate, DateTime? toDate, int? user, int? projectModule)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            var (isAdmin, scopeUserId) = await ResolveReportScopeAsync();
            int? effectiveUser = isAdmin ? user : scopeUserId;

            var items = await _reportRepo.GetProjectEffortReportAsync(fromDate, toDate, effectiveUser, projectModule);
            var users = isAdmin ? (await _taskRepo.GetAssignableUsersAsync()).ToList() : new();
            var projects = (await _taskRepo.GetProjectsAsync()).ToList();

            var vm = new ProjectEffortReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                UserFilter = user,
                ProjectModuleFilter = projectModule,
                IsAdminView = isAdmin,
                Items = items.ToList(),
                Users = users,
                ProjectModules = projects
            };

            return View(vm);
        }
    }
}

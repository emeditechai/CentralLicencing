using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PayoutConfigurationController : Controller
    {
        private readonly IPayoutConfigurationRepository _configRepo;
        private readonly IUserRepository _userRepo;
        private readonly IDailyTaskLogRepository _taskRepo;

        public PayoutConfigurationController(
            IPayoutConfigurationRepository configRepo,
            IUserRepository userRepo,
            IDailyTaskLogRepository taskRepo)
        {
            _configRepo = configRepo;
            _userRepo = userRepo;
            _taskRepo = taskRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── Index: list all employees with payout config status ────
        public async Task<IActionResult> Index(string? search, string? model, string? status)
        {
            var users = await _configRepo.GetAllConfigurationsAsync(search, model, status);
            var vm = new PayoutConfigurationIndexViewModel
            {
                Users = users.ToList(),
                SearchTerm = search,
                ModelFilter = model,
                StatusFilter = status
            };
            return View(vm);
        }

        // ── Setup GET: config form for a user ──────────────────────
        public async Task<IActionResult> Setup(int userId)
        {
            if (userId <= 0) return BadRequest();
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var existing = await _configRepo.GetConfigurationByUserIdAsync(userId);
            var vm = new PayoutConfigurationFormViewModel
            {
                UserId = userId,
                UserName = user.FullName ?? user.Username,
                EmployeeCode = user.EmployeeCode,
                DepartmentName = user.DepartmentName,
                IsEdit = existing != null,
                PayoutModel = existing?.PayoutModel ?? "Hourly",
                HourlyRate = existing?.HourlyRate,
                DefaultCommissionAmount = existing?.DefaultCommissionAmount,
                EffectiveFrom = existing?.EffectiveFrom ?? DateTime.Today
            };
            return View(vm);
        }

        // ── Setup POST: upsert configuration ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(PayoutConfigurationFormViewModel vm)
        {
            // Clear ALL auto-generated model state; we validate manually below
            ModelState.Clear();

            // Manual validation — only the fields we care about
            if (string.IsNullOrWhiteSpace(vm.PayoutModel))
                ModelState.AddModelError(nameof(vm.PayoutModel), "Payout model is required.");
            if (vm.PayoutModel == "Hourly" && (!vm.HourlyRate.HasValue || vm.HourlyRate <= 0))
                ModelState.AddModelError(nameof(vm.HourlyRate), "Hourly rate is required and must be greater than zero.");
            if (vm.PayoutModel == "Commission" && (!vm.DefaultCommissionAmount.HasValue || vm.DefaultCommissionAmount <= 0))
                ModelState.AddModelError(nameof(vm.DefaultCommissionAmount), "Default commission amount is required and must be greater than zero.");
            if (vm.EffectiveFrom == default)
                ModelState.AddModelError(nameof(vm.EffectiveFrom), "Effective date is required.");
            if (vm.UserId <= 0)
                ModelState.AddModelError(nameof(vm.UserId), "Invalid user.");

            if (!ModelState.IsValid)
            {
                var user = await _userRepo.GetByIdAsync(vm.UserId);
                vm.UserName = user?.FullName ?? user?.Username ?? "";
                vm.EmployeeCode = user?.EmployeeCode;
                vm.DepartmentName = user?.DepartmentName;
                return View(vm);
            }

            var config = new PayoutConfiguration
            {
                UserId = vm.UserId,
                PayoutModel = vm.PayoutModel,
                HourlyRate = vm.HourlyRate,
                DefaultCommissionAmount = vm.DefaultCommissionAmount,
                EffectiveFrom = vm.EffectiveFrom,
                CreatedById = CurrentUserId
            };

            var success = await _configRepo.UpsertConfigurationAsync(config);
            TempData[success ? "Success" : "Error"] = success
                ? "Payout configuration saved successfully."
                : "Failed to save payout configuration.";

            return RedirectToAction(nameof(Index));
        }

        // ── Commission Rules: list + CRUD ──────────────────────────
        public async Task<IActionResult> CommissionRules(int userId)
        {
            if (userId <= 0) return BadRequest();
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var config = await _configRepo.GetConfigurationByUserIdAsync(userId);
            if (config == null || config.PayoutModel != "Commission")
            {
                TempData["Error"] = "Commission rules are only available for users configured with the Commission payout model.";
                return RedirectToAction(nameof(Index));
            }

            var rules = await _configRepo.GetCommissionRulesAsync(userId);
            var taskTypes = await _taskRepo.GetTaskTypesAsync();
            var categories = await _taskRepo.GetTaskCategoriesAsync();
            var projects = await _taskRepo.GetProjectsAsync();

            var vm = new PayoutCommissionRulesViewModel
            {
                UserId = userId,
                UserName = user.FullName ?? user.Username,
                EmployeeCode = user.EmployeeCode,
                DefaultCommissionAmount = config.DefaultCommissionAmount,
                Rules = rules.ToList(),
                TaskTypes = taskTypes.ToList(),
                TaskCategories = categories.ToList(),
                Projects = projects.ToList()
            };
            return View(vm);
        }

        // ── AddRule (AJAX POST) ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRule([FromForm] PayoutCommissionRuleFormViewModel form)
        {
            if (form.Amount <= 0)
                return Json(new { success = false, message = "Amount must be greater than zero." });

            var rule = new PayoutCommissionRule
            {
                UserId = form.UserId,
                TaskTypeId = form.TaskTypeId,
                TaskCategoryId = form.TaskCategoryId,
                ProjectModuleId = form.ProjectModuleId,
                Amount = form.Amount,
                EffectiveFrom = form.EffectiveFrom,
                CreatedById = CurrentUserId
            };

            var id = await _configRepo.AddCommissionRuleAsync(rule);
            return Json(new { success = id > 0, message = id > 0 ? "Rule added." : "Failed to add rule." });
        }

        // ── EditRule (AJAX POST) ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRule([FromForm] PayoutCommissionRuleFormViewModel form)
        {
            if (form.Id <= 0) return Json(new { success = false, message = "Invalid rule." });
            if (form.Amount <= 0)
                return Json(new { success = false, message = "Amount must be greater than zero." });

            var rule = new PayoutCommissionRule
            {
                Id = form.Id,
                UserId = form.UserId,
                TaskTypeId = form.TaskTypeId,
                TaskCategoryId = form.TaskCategoryId,
                ProjectModuleId = form.ProjectModuleId,
                Amount = form.Amount,
                EffectiveFrom = form.EffectiveFrom
            };

            var ok = await _configRepo.UpdateCommissionRuleAsync(rule);
            return Json(new { success = ok, message = ok ? "Rule updated." : "Failed to update rule." });
        }

        // ── DeleteRule (AJAX POST) ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRule(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid rule." });
            var ok = await _configRepo.DeleteCommissionRuleAsync(id);
            return Json(new { success = ok, message = ok ? "Rule deleted." : "Failed to delete rule." });
        }
    }
}

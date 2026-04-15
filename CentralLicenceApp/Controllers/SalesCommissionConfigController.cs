using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SalesCommissionConfigController : Controller
    {
        private readonly ISalesCommissionConfigRepository _configRepo;
        private readonly IUserRepository _userRepo;
        private readonly IProductMasterRepository _productRepo;

        public SalesCommissionConfigController(
            ISalesCommissionConfigRepository configRepo,
            IUserRepository userRepo,
            IProductMasterRepository productRepo)
        {
            _configRepo = configRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── Index: list all users with config status ───────────────
        public async Task<IActionResult> Index(string? search)
        {
            var configs = await _configRepo.GetAllConfigurationsAsync(search, null, null);
            var vm = new SalesCommConfigIndexViewModel
            {
                Users = configs.ToList(),
                SearchTerm = search
            };
            return View(vm);
        }

        // ── Setup GET ──────────────────────────────────────────────
        public async Task<IActionResult> Setup(int userId)
        {
            if (userId <= 0) return BadRequest();
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var existing = await _configRepo.GetConfigurationByUserIdAsync(userId);
            var vm = new SalesCommConfigFormViewModel
            {
                UserId = userId,
                UserName = user.FullName ?? user.Username,
                EmployeeCode = user.EmployeeCode,
                DepartmentName = user.DepartmentName,
                IsEdit = existing != null,
                CommissionType = existing?.CommissionType ?? "Percentage",
                DefaultRate = existing?.DefaultRate ?? 0,
                EffectiveFrom = existing?.EffectiveFrom ?? DateTime.Today
            };
            return View(vm);
        }

        // ── Setup POST ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(SalesCommConfigFormViewModel vm)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(vm.CommissionType))
                ModelState.AddModelError(nameof(vm.CommissionType), "Commission type is required.");
            if (vm.DefaultRate <= 0)
                ModelState.AddModelError(nameof(vm.DefaultRate), "Default rate is required and must be greater than zero.");
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

            var config = new SalesCommissionConfiguration
            {
                UserId = vm.UserId,
                CommissionType = vm.CommissionType,
                DefaultRate = vm.DefaultRate,
                EffectiveFrom = vm.EffectiveFrom,
                CreatedById = CurrentUserId
            };

            var success = await _configRepo.UpsertConfigurationAsync(config);
            TempData[success ? "Success" : "Error"] = success
                ? "Sales commission configuration saved successfully."
                : "Failed to save configuration.";

            return RedirectToAction(nameof(Index));
        }

        // ── Rules: list product-specific overrides ─────────────────
        public async Task<IActionResult> Rules(int userId)
        {
            if (userId <= 0) return BadRequest();
            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null) return NotFound();

            var config = await _configRepo.GetConfigurationByUserIdAsync(userId);
            if (config == null)
            {
                TempData["Error"] = "Please set up commission configuration for this user first.";
                return RedirectToAction(nameof(Index));
            }

            var rules = await _configRepo.GetRulesAsync(userId);
            var products = await _productRepo.GetAllAsync();

            var vm = new SalesCommRulesViewModel
            {
                UserId = userId,
                UserName = user.FullName ?? user.Username,
                EmployeeCode = user.EmployeeCode,
                DefaultRate = config.DefaultRate,
                CommissionType = config.CommissionType,
                Rules = rules.ToList(),
                Products = products.ToList()
            };
            return View(vm);
        }

        // ── AddRule (AJAX POST) ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRule([FromForm] SalesCommRuleFormViewModel form)
        {
            if (form.Rate <= 0)
                return Json(new { success = false, message = "Rate must be greater than zero." });

            var rule = new SalesCommissionRule
            {
                UserId = form.UserId,
                ProductId = form.ProductId,
                CommissionType = form.CommissionType,
                Rate = form.Rate,
                EffectiveFrom = form.EffectiveFrom,
                CreatedById = CurrentUserId
            };

            var id = await _configRepo.AddRuleAsync(rule);
            return Json(new { success = id > 0, message = id > 0 ? "Rule added." : "Failed to add rule." });
        }

        // ── EditRule (AJAX POST) ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRule([FromForm] SalesCommRuleFormViewModel form)
        {
            if (form.Id <= 0) return Json(new { success = false, message = "Invalid rule." });
            if (form.Rate <= 0)
                return Json(new { success = false, message = "Rate must be greater than zero." });

            var rule = new SalesCommissionRule
            {
                Id = form.Id,
                UserId = form.UserId,
                ProductId = form.ProductId,
                CommissionType = form.CommissionType,
                Rate = form.Rate,
                EffectiveFrom = form.EffectiveFrom
            };

            var ok = await _configRepo.UpdateRuleAsync(rule);
            return Json(new { success = ok, message = ok ? "Rule updated." : "Failed to update rule." });
        }

        // ── DeleteRule (AJAX POST) ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRule(int id)
        {
            if (id <= 0) return Json(new { success = false, message = "Invalid rule." });
            var ok = await _configRepo.DeleteRuleAsync(id);
            return Json(new { success = ok, message = ok ? "Rule deleted." : "Failed to delete rule." });
        }
    }
}

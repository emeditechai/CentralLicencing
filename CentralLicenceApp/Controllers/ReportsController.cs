using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IClientLicenseRepository _clientLicenseRepository;
        private readonly IClientDetailsReportExportService _clientDetailsReportExportService;
        private readonly IUserRepository _userRepository;
        private readonly IExpenseReportExportService _expenseReportExportService;
        private readonly ISettlementReportExportService _settlementReportExportService;

        public ReportsController(
            IReportRepository reportRepository,
            IClientLicenseRepository clientLicenseRepository,
            IClientDetailsReportExportService clientDetailsReportExportService,
            IUserRepository userRepository,
            IExpenseReportExportService expenseReportExportService,
            ISettlementReportExportService settlementReportExportService)
        {
            _reportRepository = reportRepository;
            _clientLicenseRepository = clientLicenseRepository;
            _clientDetailsReportExportService = clientDetailsReportExportService;
            _userRepository = userRepository;
            _expenseReportExportService = expenseReportExportService;
            _settlementReportExportService = settlementReportExportService;
        }

        public async Task<IActionResult> ClientDetails(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");
            }

            var items = ModelState.IsValid
                ? await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType)
                : Array.Empty<CentralLicenceApp.Models.Reports.ClientDetailsReportRow>();

            var vm = new ClientDetailsReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProductType = productType,
                ProductTypes = (await _clientLicenseRepository.GetDistinctProductTypesAsync()).ToList(),
                Items = items.ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> ExportClientDetailsExcel(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ClientDetails), new { fromDate, toDate, productType });
            }

            var items = await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType);
            var bytes = _clientDetailsReportExportService.GenerateExcel(items, productType, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ClientDetailsReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportClientDetailsPdf(DateTime? fromDate, DateTime? toDate, string? productType)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ClientDetails), new { fromDate, toDate, productType });
            }

            var items = await _reportRepository.GetClientDetailsReportAsync(fromDate, toDate, productType);
            var bytes = _clientDetailsReportExportService.GeneratePdf(items, productType, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"ClientDetailsReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        private static string? ValidateDateRange(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                return "From Date cannot be later than To Date.";
            }

            return null;
        }

        private static string? FormatDate(DateTime? value)
        {
            return value?.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        // ── Expense Report ────────────────────────────────────────────────────────

        public async Task<IActionResult> ExpenseReport(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");

            const int pageSize = 20;
            var (isAdmin, userId) = await ResolveReportScopeAsync();

            var (items, totalCount) = ModelState.IsValid
                ? await _reportRepository.GetExpenseReportAsync(fromDate, toDate, userId, page, pageSize)
                : (Array.Empty<CentralLicenceApp.Models.Reports.ExpenseReportRow>(), 0);

            var vm = new ExpenseReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Items = items.ToList(),
                IsAdminView = isAdmin,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // ── Settlement Report ─────────────────────────────────────────────────────

        public async Task<IActionResult> SettlementReport(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
                ModelState.AddModelError(string.Empty, "From Date cannot be later than To Date.");

            const int pageSize = 20;
            var (isAdmin, userId) = await ResolveReportScopeAsync();

            var (items, totalCount) = ModelState.IsValid
                ? await _reportRepository.GetSettlementReportAsync(fromDate, toDate, userId, page, pageSize)
                : (Array.Empty<CentralLicenceApp.Models.Reports.SettlementReportRow>(), 0);

            var vm = new SettlementReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Items = items.ToList(),
                IsAdminView = isAdmin,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(vm);
        }

        // ── Expense Report Exports ────────────────────────────────────────────────

        public async Task<IActionResult> ExportExpenseReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ExpenseReport), new { fromDate, toDate });
            }

            var (isAdmin, userId) = await ResolveReportScopeAsync();
            var items = await _reportRepository.GetAllExpenseReportAsync(fromDate, toDate, userId);
            var bytes = _expenseReportExportService.GenerateExcel(items, isAdmin, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExpenseReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportExpenseReportPdf(DateTime? fromDate, DateTime? toDate)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(ExpenseReport), new { fromDate, toDate });
            }

            var (isAdmin, userId) = await ResolveReportScopeAsync();
            var items = await _reportRepository.GetAllExpenseReportAsync(fromDate, toDate, userId);
            var bytes = _expenseReportExportService.GeneratePdf(items, isAdmin, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"ExpenseReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // ── Settlement Report Exports ─────────────────────────────────────────────

        public async Task<IActionResult> ExportSettlementReportExcel(DateTime? fromDate, DateTime? toDate)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(SettlementReport), new { fromDate, toDate });
            }

            var (isAdmin, userId) = await ResolveReportScopeAsync();
            var items = await _reportRepository.GetAllSettlementReportAsync(fromDate, toDate, userId);
            var bytes = _settlementReportExportService.GenerateExcel(items, isAdmin, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SettlementReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportSettlementReportPdf(DateTime? fromDate, DateTime? toDate)
        {
            var validationError = ValidateDateRange(fromDate, toDate);
            if (validationError is not null)
            {
                TempData["ReportError"] = validationError;
                return RedirectToAction(nameof(SettlementReport), new { fromDate, toDate });
            }

            var (isAdmin, userId) = await ResolveReportScopeAsync();
            var items = await _reportRepository.GetAllSettlementReportAsync(fromDate, toDate, userId);
            var bytes = _settlementReportExportService.GeneratePdf(items, isAdmin, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"SettlementReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns (isAdmin, userId).  When the caller is Administrator or super-admin,
        /// isAdmin = true and userId = null so the SP returns all rows.
        /// Otherwise userId carries the current user's Id for row-level filtering.
        /// </summary>
        private async Task<(bool isAdmin, int? userId)> ResolveReportScopeAsync()
        {
            bool isSuperAdmin = User.FindFirst("IsSuperAdmin")?.Value == "true";
            bool isAdminRole  = User.IsInRole("Administrator");

            if (isSuperAdmin || isAdminRole)
                return (true, null);

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdValue, out var parsedId))
                return (false, parsedId);

            // Fallback: look up by username
            var userName = User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var user = await _userRepository.GetByUsernameAsync(userName);
                if (user != null)
                    return (false, user.Id);
            }

            return (false, null);
        }
    }
}
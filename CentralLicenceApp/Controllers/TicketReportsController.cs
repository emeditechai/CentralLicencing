using System.Security.Claims;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Admin,Ticket Agent")]
    public class TicketReportsController : Controller
    {
        private readonly ITicketReportRepository _reportRepo;
        private readonly ITicketReportExportService _exportService;

        public TicketReportsController(ITicketReportRepository reportRepo, ITicketReportExportService exportService)
        {
            _reportRepo = reportRepo;
            _exportService = exportService;
        }

        private bool IsAdminOrTicketAdmin =>
            User.IsInRole("Administrator") || User.IsInRole("Ticket Admin");

        private int? CurrentUserId
        {
            get
            {
                var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(val, out var id) ? id : null;
            }
        }

        // ── Dashboard / Overview (Admin + Ticket Admin only) ──
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            if (!ValidateDateRange(fromDate, toDate)) return View(new TicketReportDashboardViewModel());
            var vm = await _reportRepo.GetDashboardAsync(fromDate, toDate);
            return View(vm);
        }

        // ── Agent Performance (Admins see all, Ticket Agent sees own) ──
        public async Task<IActionResult> AgentPerformance(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            if (!ValidateDateRange(fromDate, toDate)) return View(new AgentPerformanceReportViewModel());
            const int pageSize = 20;
            int? agentId = IsAdminOrTicketAdmin ? null : CurrentUserId;
            var (rows, totalCount) = await _reportRepo.GetAgentPerformanceAsync(fromDate, toDate, page, pageSize, agentId);
            var vm = new AgentPerformanceReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Agents = rows,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            return View(vm);
        }

        // ── SLA Compliance (Admin + Ticket Admin only) ──
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> SlaCompliance(DateTime? fromDate, DateTime? toDate, int page = 1)
        {
            if (!ValidateDateRange(fromDate, toDate)) return View(new SlaComplianceReportViewModel());
            const int pageSize = 20;
            var (rows, totalCount) = await _reportRepo.GetSlaComplianceAsync(fromDate, toDate, page, pageSize);
            var summary = await _reportRepo.GetSlaComplianceSummaryAsync(fromDate, toDate);
            var vm = new SlaComplianceReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                Rows = rows,
                PageNumber = page,
                PageSize = pageSize,
                TotalTickets = summary.TotalTickets,
                ResponseSlaMetCount = summary.ResponseSlaMetCount,
                ResponseSlaBreachedCount = summary.ResponseSlaBreachedCount,
                ResolutionSlaMetCount = summary.ResolutionSlaMetCount,
                ResolutionSlaBreachedCount = summary.ResolutionSlaBreachedCount
            };
            var respDenom = vm.ResponseSlaMetCount + vm.ResponseSlaBreachedCount;
            vm.ResponseSlaCompliancePercent = respDenom > 0
                ? Math.Round(vm.ResponseSlaMetCount * 100.0 / respDenom, 1) : 0;
            var resDenom = vm.ResolutionSlaMetCount + vm.ResolutionSlaBreachedCount;
            vm.ResolutionSlaCompliancePercent = resDenom > 0
                ? Math.Round(vm.ResolutionSlaMetCount * 100.0 / resDenom, 1) : 0;
            return View(vm);
        }

        // ── Agent Performance Export ──
        public async Task<IActionResult> ExportAgentPerformanceExcel(DateTime? fromDate, DateTime? toDate)
        {
            if (!ValidateDateRange(fromDate, toDate))
                return RedirectToAction(nameof(AgentPerformance), new { fromDate, toDate });
            int? agentId = IsAdminOrTicketAdmin ? null : CurrentUserId;
            var items = await _reportRepo.GetAllAgentPerformanceAsync(fromDate, toDate, agentId);
            var bytes = _exportService.GenerateAgentPerformanceExcel(items, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AgentPerformanceReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        public async Task<IActionResult> ExportAgentPerformancePdf(DateTime? fromDate, DateTime? toDate)
        {
            if (!ValidateDateRange(fromDate, toDate))
                return RedirectToAction(nameof(AgentPerformance), new { fromDate, toDate });
            int? agentId = IsAdminOrTicketAdmin ? null : CurrentUserId;
            var items = await _reportRepo.GetAllAgentPerformanceAsync(fromDate, toDate, agentId);
            var bytes = _exportService.GenerateAgentPerformancePdf(items, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"AgentPerformanceReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        // ── SLA Compliance Export (Admin + Ticket Admin only) ──
        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> ExportSlaComplianceExcel(DateTime? fromDate, DateTime? toDate)
        {
            if (!ValidateDateRange(fromDate, toDate))
                return RedirectToAction(nameof(SlaCompliance), new { fromDate, toDate });
            var items = await _reportRepo.GetAllSlaComplianceAsync(fromDate, toDate);
            var bytes = _exportService.GenerateSlaComplianceExcel(items, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SlaComplianceReport-{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [Authorize(Roles = "Administrator,Ticket Admin")]
        public async Task<IActionResult> ExportSlaCompliancePdf(DateTime? fromDate, DateTime? toDate)
        {
            if (!ValidateDateRange(fromDate, toDate))
                return RedirectToAction(nameof(SlaCompliance), new { fromDate, toDate });
            var items = await _reportRepo.GetAllSlaComplianceAsync(fromDate, toDate);
            var bytes = _exportService.GenerateSlaCompliancePdf(items, FormatDate(fromDate), FormatDate(toDate));
            return File(bytes, "application/pdf", $"SlaComplianceReport-{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        private bool ValidateDateRange(DateTime? from, DateTime? to)
        {
            if (from.HasValue && to.HasValue && from > to)
            {
                TempData["Error"] = "From date cannot be after To date.";
                return false;
            }
            return true;
        }

        private static string? FormatDate(DateTime? d) => d?.ToString("dd-MMM-yyyy");
    }
}

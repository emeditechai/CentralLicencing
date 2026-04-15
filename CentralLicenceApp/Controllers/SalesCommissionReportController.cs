using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SalesCommissionReportController : Controller
    {
        private readonly ISalesCommissionReportRepository _reportRepo;
        private readonly IUserRepository _userRepo;

        public SalesCommissionReportController(ISalesCommissionReportRepository reportRepo, IUserRepository userRepo)
        {
            _reportRepo = reportRepo;
            _userRepo = userRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private bool IsAdmin => User.IsInRole("Administrator");

        // ── Summary ────────────────────────────────────────────────
        public async Task<IActionResult> Summary(DateTime? from, DateTime? to, int? userId)
        {
            var items = await _reportRepo.GetSummaryReportAsync(from, to, userId);
            var users = await _userRepo.GetEmployeesAsync();

            var vm = new SalesCommSummaryReportViewModel
            {
                Items = items.ToList(),
                Users = users.ToList(),
                FromDate = from,
                ToDate = to,
                UserFilter = userId,
                IsAdminView = IsAdmin
            };
            return View(vm);
        }

        // ── Detail ─────────────────────────────────────────────────
        public async Task<IActionResult> Detail(DateTime? from, DateTime? to, int? userId, int? batchId)
        {
            var items = await _reportRepo.GetDetailReportAsync(from, to, userId, batchId);
            var users = await _userRepo.GetEmployeesAsync();

            var vm = new SalesCommDetailReportViewModel
            {
                Items = items.ToList(),
                Users = users.ToList(),
                FromDate = from,
                ToDate = to,
                UserFilter = userId,
                BatchIdFilter = batchId,
                IsAdminView = IsAdmin
            };
            return View(vm);
        }

        // ── History ────────────────────────────────────────────────
        public async Task<IActionResult> History(DateTime? from, DateTime? to, int? userId, string? status)
        {
            var items = await _reportRepo.GetHistoryReportAsync(from, to, userId, status);
            var users = await _userRepo.GetEmployeesAsync();

            var vm = new SalesCommHistoryReportViewModel
            {
                Items = items.ToList(),
                Users = users.ToList(),
                FromDate = from,
                ToDate = to,
                UserFilter = userId,
                StatusFilter = status,
                IsAdminView = IsAdmin
            };
            return View(vm);
        }
    }
}

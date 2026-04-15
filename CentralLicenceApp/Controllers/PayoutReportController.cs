using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator,Ticket Agent")]
    public class PayoutReportController : Controller
    {
        private readonly IPayoutReportRepository _reportRepo;
        private readonly IUserRepository _userRepo;

        public PayoutReportController(IPayoutReportRepository reportRepo, IUserRepository userRepo)
        {
            _reportRepo = reportRepo;
            _userRepo = userRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private bool IsAdmin => User.IsInRole("Administrator");

        // ── Summary Report ─────────────────────────────────────────
        public async Task<IActionResult> Summary(DateTime? from, DateTime? to, int? userId)
        {
            // Non-admin users can only see their own data
            if (!IsAdmin) userId = CurrentUserId;

            var items = await _reportRepo.GetSummaryReportAsync(from, to, userId);
            var users = IsAdmin ? (await _userRepo.GetEmployeesAsync()).ToList() : new();

            var vm = new PayoutSummaryReportViewModel
            {
                Items = items.ToList(),
                Users = users,
                FromDate = from,
                ToDate = to,
                UserFilter = userId,
                IsAdminView = IsAdmin
            };
            return View(vm);
        }

        // ── Detail Report ──────────────────────────────────────────
        public async Task<IActionResult> Detail(DateTime? from, DateTime? to, int? userId, int? batchId)
        {
            if (!IsAdmin) userId = CurrentUserId;

            var items = await _reportRepo.GetDetailReportAsync(from, to, userId, batchId);
            var users = IsAdmin ? (await _userRepo.GetEmployeesAsync()).ToList() : new();

            var vm = new PayoutDetailReportViewModel
            {
                Items = items.ToList(),
                Users = users,
                FromDate = from,
                ToDate = to,
                UserFilter = userId,
                BatchIdFilter = batchId,
                IsAdminView = IsAdmin
            };
            return View(vm);
        }

        // ── History Report ─────────────────────────────────────────
        public async Task<IActionResult> History(DateTime? from, DateTime? to, int? userId, string? status)
        {
            if (!IsAdmin) userId = CurrentUserId;

            var items = await _reportRepo.GetHistoryReportAsync(from, to, userId, status);
            var users = IsAdmin ? (await _userRepo.GetEmployeesAsync()).ToList() : new();

            var vm = new PayoutHistoryReportViewModel
            {
                Items = items.ToList(),
                Users = users,
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

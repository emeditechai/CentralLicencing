using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IClientLicenseRepository _licenseRepo;
        private readonly IExpenseRequestRepository _expenseRequestRepo;
        private readonly IUserRepository _userRepo;
        private readonly IClientDetailsRepository _clientDetailsRepo;

        public DashboardController(
            IClientLicenseRepository licenseRepo,
            IExpenseRequestRepository expenseRequestRepo,
            IUserRepository userRepo,
            IClientDetailsRepository clientDetailsRepo)
        {
            _licenseRepo        = licenseRepo;
            _expenseRequestRepo = expenseRequestRepo;
            _userRepo           = userRepo;
            _clientDetailsRepo  = clientDetailsRepo;
        }

        private static readonly HashSet<string> CrmOnlyRoles = new(StringComparer.OrdinalIgnoreCase)
            { "ClientTicket", "Ticket Admin", "Ticket Agent" };

        public async Task<IActionResult> Index(string? productType)
        {
            var currentRole = User.FindFirstValue(ClaimTypes.Role) ?? "";
            if (CrmOnlyRoles.Contains(currentRole))
                return RedirectToAction("MyTickets", "HelpDeskTicket");

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Challenge();
            }

            var vm = await _licenseRepo.GetDashboardStatsAsync(productType);
            var expenseCounts = HasSuperAdminAccess()
                ? await _expenseRequestRepo.GetDashboardCountsAsync()
                : await _expenseRequestRepo.GetDashboardCountsAsync(await _userRepo.GetSelfAndSubordinateIdsAsync(currentUser.Id));

            vm.ApprovedExpenseRequests = expenseCounts.Approved;
            vm.ReimbursementInProcessRequests = expenseCounts.ReimbursementInProcess;
            vm.SettledExpenseRequests = expenseCounts.Settled;
            vm.SelectedProductType    = productType;
            vm.AvailableProductTypes  = (await _licenseRepo.GetDistinctProductTypesAsync()).ToList();
            vm.SubscriptionReminders  = (await _clientDetailsRepo.GetSubscriptionRemindersAsync(5)).ToList();
            ViewBag.IsCoreMember = currentUser.IsCoreMember;
            return View(vm);
        }

        private async Task<UserMaster?> GetCurrentUserAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId)) return null;
            return await _userRepo.GetByIdAsync(userId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRenewed(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || !currentUser.IsCoreMember)
                return Json(new { success = false, message = "Only Core Members can mark a subscription as renewed." });

            await _clientDetailsRepo.MarkRenewedAsync(id);
            return Json(new { success = true });
        }

        private bool HasSuperAdminAccess()
        {
            return string.Equals(User.Identity?.Name, "admin", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

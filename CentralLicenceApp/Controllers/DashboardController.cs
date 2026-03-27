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

        public DashboardController(IClientLicenseRepository licenseRepo, IExpenseRequestRepository expenseRequestRepo, IUserRepository userRepo)
        {
            _licenseRepo = licenseRepo;
            _expenseRequestRepo = expenseRequestRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index(string? productType)
        {
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
            return View(vm);
        }

        private async Task<UserMaster?> GetCurrentUserAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId)) return null;
            return await _userRepo.GetByIdAsync(userId);
        }

        private bool HasSuperAdminAccess()
        {
            return string.Equals(User.Identity?.Name, "admin", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}

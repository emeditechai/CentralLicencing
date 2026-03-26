using System.Threading.Tasks;
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

        public DashboardController(IClientLicenseRepository licenseRepo, IExpenseRequestRepository expenseRequestRepo)
        {
            _licenseRepo = licenseRepo;
            _expenseRequestRepo = expenseRequestRepo;
        }

        public async Task<IActionResult> Index(string? productType)
        {
            var vm = await _licenseRepo.GetDashboardStatsAsync(productType);
            var expenseCounts = await _expenseRequestRepo.GetDashboardCountsAsync();
            vm.ApprovedExpenseRequests = expenseCounts.Approved;
            vm.ReimbursementInProcessRequests = expenseCounts.ReimbursementInProcess;
            vm.SettledExpenseRequests = expenseCounts.Settled;
            vm.SelectedProductType    = productType;
            vm.AvailableProductTypes  = (await _licenseRepo.GetDistinctProductTypesAsync()).ToList();
            return View(vm);
        }
    }
}

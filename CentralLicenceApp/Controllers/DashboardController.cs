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

        public DashboardController(IClientLicenseRepository licenseRepo)
        {
            _licenseRepo = licenseRepo;
        }

        public async Task<IActionResult> Index(string? productType)
        {
            var vm = await _licenseRepo.GetDashboardStatsAsync(productType);
            vm.SelectedProductType    = productType;
            vm.AvailableProductTypes  = (await _licenseRepo.GetDistinctProductTypesAsync()).ToList();
            return View(vm);
        }
    }
}

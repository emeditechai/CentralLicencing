using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class LicenseHistoryController : Controller
    {
        private readonly ILicenseHistoryRepository _repo;
        private readonly IClientLicenseRepository _licenseRepo;

        public LicenseHistoryController(ILicenseHistoryRepository repo, IClientLicenseRepository licenseRepo)
        {
            _repo        = repo;
            _licenseRepo = licenseRepo;
        }

        public async Task<IActionResult> Index(string? search, string? valid, string? productType, int page = 1)
        {
            const int pageSize = 15;
            var (items, total) = await _repo.GetPagedAsync(search, valid, productType, page, pageSize);

            ViewBag.Search               = search;
            ViewBag.Valid                = valid;
            ViewBag.ProductType          = productType;
            ViewBag.Page                 = page;
            ViewBag.PageSize             = pageSize;
            ViewBag.Total                = total;
            ViewBag.TotalPages           = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.AvailableProductTypes = (await _licenseRepo.GetDistinctProductTypesAsync()).ToList();

            return View(items.ToList());
        }
    }
}

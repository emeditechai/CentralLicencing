using System.Threading.Tasks;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class LicenseAuditLogController : Controller
    {
        private readonly IClientLicenseAuditLogRepository _auditRepo;

        public LicenseAuditLogController(IClientLicenseAuditLogRepository auditRepo)
        {
            _auditRepo = auditRepo;
        }

        public async Task<IActionResult> Index(string? search, string? field, int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _auditRepo.GetPagedAsync(search, field, page, pageSize);

            ViewBag.Search     = search;
            ViewBag.Field      = field;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;
            ViewBag.Total      = total;
            ViewBag.TotalPages = (int)System.Math.Ceiling(total / (double)pageSize);

            return View(items);
        }
    }
}

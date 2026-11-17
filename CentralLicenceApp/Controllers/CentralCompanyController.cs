using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    /// <summary>Licences › Central Company: read-only view of Central_Company and Central_Users. Administrators only.</summary>
    [Authorize(Roles = "Administrator")]
    public class CentralCompanyController : Controller
    {
        private readonly ICentralCompanyRepository _repo;
        private readonly ILogger<CentralCompanyController> _logger;

        public CentralCompanyController(ICentralCompanyRepository repo, ILogger<CentralCompanyController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search)
        {
            ViewBag.Search = search;
            return View(await _repo.GetCompaniesAsync(search));
        }

        public async Task<IActionResult> Details(int id, string? search)
        {
            var company = await _repo.GetCompanyAsync(id);
            if (company == null) return NotFound();
            ViewBag.Search = search;
            ViewBag.Users = await _repo.GetCompanyUsersAsync(id, search);
            return View(company);
        }

        // Returns the decrypted connection string on request only, so it is never in the page source or browser cache.
        [HttpPost, ValidateAntiForgeryToken]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ConnectionString(int id)
        {
            if (await _repo.GetCompanyAsync(id) == null) return NotFound();
            var value = await _repo.GetDecryptedConnectionStringAsync(id);
            _logger.LogInformation("Decrypted connection string of Central_Company {CompanyId} viewed by {User}", id, User.Identity?.Name);
            return Json(new { success = true, value = string.IsNullOrWhiteSpace(value) ? null : value });
        }

        public async Task<IActionResult> UserDetails(long id)
        {
            var user = await _repo.GetUserAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}

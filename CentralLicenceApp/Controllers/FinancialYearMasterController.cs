using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class FinancialYearMasterController : Controller
    {
        private readonly IFinancialYearMasterRepository _repo;

        public FinancialYearMasterController(IFinancialYearMasterRepository repo)
        {
            _repo = repo;
        }

        // GET /FinancialYearMaster
        public async Task<IActionResult> Index()
        {
            // Auto-sync: mark the FY whose date range contains today as current
            await _repo.SyncCurrentFYAsync();
            var items = await _repo.GetAllAsync();
            return View(items);
        }

        // POST /FinancialYearMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction(nameof(Index));
            }

            var fyCode = GenerateFYCode(startDate, endDate);

            if (await _repo.FYCodeExistsAsync(fyCode))
            {
                TempData["Error"] = $"Financial year <strong>{fyCode}</strong> already exists.";
                return RedirectToAction(nameof(Index));
            }

            var fy = new FinancialYearMaster
            {
                StartDate = startDate,
                EndDate = endDate,
                FYCode = fyCode,
                IsActive = true
            };

            await _repo.CreateAsync(fy);
            TempData["Success"] = $"Financial year <strong>{fyCode}</strong> created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST /FinancialYearMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToAction(nameof(Index));
            }

            var fy = await _repo.GetByIdAsync(id);
            if (fy == null) return NotFound();

            var fyCode = GenerateFYCode(startDate, endDate);

            if (await _repo.FYCodeExistsAsync(fyCode, id))
            {
                TempData["Error"] = $"Financial year <strong>{fyCode}</strong> already exists.";
                return RedirectToAction(nameof(Index));
            }

            fy.StartDate = startDate;
            fy.EndDate = endDate;
            fy.FYCode = fyCode;
            await _repo.UpdateAsync(fy);

            TempData["Success"] = $"Financial year <strong>{fyCode}</strong> updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST /FinancialYearMaster/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var fy = await _repo.GetByIdAsync(id);
            if (fy == null) return NotFound();

            await _repo.ToggleActiveAsync(id);
            TempData["Success"] = $"Financial year <strong>{fy.FYCode}</strong> is now {(fy.IsActive ? "inactive" : "active")}.";
            return RedirectToAction(nameof(Index));
        }

        // POST /FinancialYearMaster/SetCurrentFY/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetCurrentFY(int id)
        {
            var fy = await _repo.GetByIdAsync(id);
            if (fy == null) return NotFound();

            if (!fy.IsActive)
            {
                TempData["Error"] = "Cannot mark an inactive financial year as current.";
                return RedirectToAction(nameof(Index));
            }

            await _repo.SetCurrentFYAsync(id);
            TempData["Success"] = $"<strong>FY {fy.FYCode}</strong> is now marked as the current financial year.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Auto-generates FY code from start and end years (e.g. "25-26", "26-27").
        /// </summary>
        private static string GenerateFYCode(DateTime startDate, DateTime endDate)
        {
            return $"{startDate.Year % 100:D2}-{endDate.Year % 100:D2}";
        }
    }
}

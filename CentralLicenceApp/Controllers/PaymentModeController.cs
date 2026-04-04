using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class PaymentModeController : Controller
    {
        private readonly IPaymentModeRepository _repo;

        public PaymentModeController(IPaymentModeRepository repo)
        {
            _repo = repo;
        }

        // GET /PaymentMode
        public async Task<IActionResult> Index()
        {
            var modes = await _repo.GetAllAsync();
            return View(modes);
        }

        // POST /PaymentMode/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(string name, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Payment mode name is required.";
                return RedirectToAction(nameof(Index));
            }

            var mode = new PaymentMode
            {
                Name      = name.Trim(),
                IsActive  = true,
                SortOrder = sortOrder
            };

            await _repo.CreateAsync(mode);
            TempData["Success"] = "Payment mode added.";
            return RedirectToAction(nameof(Index));
        }

        // POST /PaymentMode/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, string name, int sortOrder)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Payment mode name is required.";
                return RedirectToAction(nameof(Index));
            }

            var mode = await _repo.GetByIdAsync(id);
            if (mode == null) return NotFound();

            mode.Name      = name.Trim();
            mode.SortOrder = sortOrder;
            await _repo.UpdateAsync(mode);

            TempData["Success"] = "Payment mode updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST /PaymentMode/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var mode = await _repo.GetByIdAsync(id);
            if (mode == null) return NotFound();

            await _repo.ToggleActiveAsync(id);
            TempData["Success"] = $"Payment mode '{mode.Name}' is now {(mode.IsActive ? "inactive" : "active")}.";
            return RedirectToAction(nameof(Index));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SalesInvoiceAssignmentController : Controller
    {
        private readonly ISalesInvoiceAssignmentRepository _assignRepo;
        private readonly IUserRepository _userRepo;
        private readonly IProductMasterRepository _productRepo;

        public SalesInvoiceAssignmentController(
            ISalesInvoiceAssignmentRepository assignRepo,
            IUserRepository userRepo,
            IProductMasterRepository productRepo)
        {
            _assignRepo = assignRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── Index ──────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? salesUserId, DateTime? from, DateTime? to)
        {
            var assignments = await _assignRepo.GetAssignmentsAsync(salesUserId, from, to);
            var unassigned = await _assignRepo.GetUnassignedInvoicesAsync(from, to);
            var users = await _userRepo.GetEmployeesAsync();
            var products = await _productRepo.GetAllAsync();

            var vm = new SalesInvoiceAssignmentIndexViewModel
            {
                Assignments = assignments.ToList(),
                UnassignedInvoices = unassigned.ToList(),
                SalesUsers = users.ToList(),
                Products = products.ToList(),
                SalesUserFilter = salesUserId,
                FromDate = from,
                ToDate = to
            };
            return View(vm);
        }

        // ── Assign (POST) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int invoiceId, int salesUserId, int? productId)
        {
            if (invoiceId <= 0 || salesUserId <= 0)
            {
                TempData["Error"] = "Invalid invoice or sales user.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _assignRepo.AssignAsync(invoiceId, salesUserId, productId, CurrentUserId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Invoice assigned successfully."
                : "Failed to assign. The invoice may already be assigned.";
            return RedirectToAction(nameof(Index));
        }

        // ── Unassign (POST) ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid assignment.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _assignRepo.UnassignAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Invoice unassigned."
                : "Cannot unassign. The invoice may be part of an active commission batch.";
            return RedirectToAction(nameof(Index));
        }

        // ── Reassign (POST) ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reassign(int id, int newSalesUserId, int? newProductId)
        {
            if (id <= 0 || newSalesUserId <= 0)
            {
                TempData["Error"] = "Invalid parameters.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _assignRepo.ReassignAsync(id, newSalesUserId, newProductId, CurrentUserId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Invoice reassigned successfully."
                : "Cannot reassign. The invoice may be part of an active commission batch.";
            return RedirectToAction(nameof(Index));
        }
    }
}

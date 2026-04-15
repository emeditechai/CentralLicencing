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
        private readonly ISalesCommissionConfigRepository _configRepo;

        public SalesInvoiceAssignmentController(
            ISalesInvoiceAssignmentRepository assignRepo,
            IUserRepository userRepo,
            IProductMasterRepository productRepo,
            ISalesCommissionConfigRepository configRepo)
        {
            _assignRepo = assignRepo;
            _userRepo = userRepo;
            _productRepo = productRepo;
            _configRepo = configRepo;
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

        // ── Get Invoice Line Items (AJAX) ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetInvoiceItems(int invoiceId)
        {
            if (invoiceId <= 0)
                return Json(new { success = false, message = "Invalid invoice." });

            var items = await _assignRepo.GetInvoiceLineItemsAsync(invoiceId);
            return Json(new { success = true, items });
        }

        // ── Get Default Commission Config for a Sales User (AJAX) ──
        [HttpGet]
        public async Task<IActionResult> GetUserCommissionConfig(int userId)
        {
            if (userId <= 0)
                return Json(new { success = false });

            var config = await _configRepo.GetConfigurationByUserIdAsync(userId);
            if (config == null)
                return Json(new { success = true, commissionType = "Percentage", defaultRate = 0m });

            // Also load product-specific rules
            var rules = await _configRepo.GetRulesAsync(userId);

            return Json(new
            {
                success = true,
                commissionType = config.CommissionType,
                defaultRate = config.DefaultRate,
                rules = rules.Select(r => new
                {
                    productId = r.ProductId,
                    productName = r.ProductName,
                    commissionType = r.CommissionType,
                    rate = r.Rate
                })
            });
        }

        // ── Assign (POST) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign([FromForm] AssignInvoiceRequest request)
        {
            if (request.InvoiceId <= 0 || request.SalesUserId <= 0 || request.Lines == null || !request.Lines.Any())
            {
                TempData["Error"] = "Invalid request. Please select at least one line item.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _assignRepo.AssignWithLinesAsync(request, CurrentUserId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Invoice assigned successfully."
                : "Failed to assign. This sales person may already be assigned to this invoice.";
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

        // ── Get Assignment Lines (AJAX) ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAssignmentLines(int assignmentId)
        {
            if (assignmentId <= 0)
                return Json(new { success = false });

            var lines = await _assignRepo.GetAssignmentLinesAsync(assignmentId);
            return Json(new
            {
                success = true,
                lines = lines.Select(l => new
                {
                    l.InvoiceLineId,
                    l.ItemDescription,
                    l.NetAmount,
                    l.CommissionType,
                    l.CommissionRate,
                    l.CommissionAmount
                })
            });
        }

        // ── Get Assignments for an Invoice (AJAX) ─────────────────
        [HttpGet]
        public async Task<IActionResult> GetInvoiceAssignments(int invoiceId)
        {
            if (invoiceId <= 0)
                return Json(new { success = false });

            var assignments = await _assignRepo.GetAssignmentsForInvoiceAsync(invoiceId);
            return Json(new
            {
                success = true,
                assignments = assignments.Select(a => new
                {
                    a.AssignmentId,
                    a.SalesUserId,
                    a.SalesUserName,
                    a.TotalCommissionAmount,
                    assignedAt = a.AssignedAt?.ToString("dd MMM yyyy")
                })
            });
        }
    }
}

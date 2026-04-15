using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SalesCommissionBatchController : Controller
    {
        private readonly ISalesCommissionBatchRepository _batchRepo;
        private readonly ISalesCommissionConfigRepository _configRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBankMasterRepository _bankRepo;
        private readonly IPaymentModeRepository _modeRepo;

        public SalesCommissionBatchController(
            ISalesCommissionBatchRepository batchRepo,
            ISalesCommissionConfigRepository configRepo,
            IUserRepository userRepo,
            IBankMasterRepository bankRepo,
            IPaymentModeRepository modeRepo)
        {
            _batchRepo = batchRepo;
            _configRepo = configRepo;
            _userRepo = userRepo;
            _bankRepo = bankRepo;
            _modeRepo = modeRepo;
        }

        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── Index ──────────────────────────────────────────────────
        public async Task<IActionResult> Index(int? userId, string? status, DateTime? from, DateTime? to)
        {
            var batches = await _batchRepo.GetBatchesAsync(userId, status, from, to);
            var users = await _userRepo.GetEmployeesAsync();

            var vm = new SalesCommBatchIndexViewModel
            {
                Batches = batches.ToList(),
                Users = users.ToList(),
                UserFilter = userId,
                StatusFilter = status,
                FromDate = from,
                ToDate = to
            };
            return View(vm);
        }

        // ── Generate GET ───────────────────────────────────────────
        public async Task<IActionResult> Generate()
        {
            var configs = await _configRepo.GetAllConfigurationsAsync(null, null, null);
            var vm = new SalesCommGenerateFormViewModel
            {
                ConfiguredUsers = configs.Where(c => c.IsConfigured).ToList(),
                FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                ToDate = DateTime.Today
            };
            return View(vm);
        }

        // ── Generate POST ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(SalesCommGenerateFormViewModel vm)
        {
            if (vm.FromDate > vm.ToDate)
                ModelState.AddModelError(nameof(vm.ToDate), "To date must be on or after From date.");

            if (!ModelState.IsValid)
            {
                var configs = await _configRepo.GetAllConfigurationsAsync(null, null, null);
                vm.ConfiguredUsers = configs.Where(c => c.IsConfigured).ToList();
                return View(vm);
            }

            try
            {
                var batchId = await _batchRepo.GenerateBatchAsync(vm.UserId, vm.FromDate, vm.ToDate, null, CurrentUserId);
                TempData["Success"] = "Sales commission batch generated successfully.";
                return RedirectToAction(nameof(Details), new { id = batchId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                var configs = await _configRepo.GetAllConfigurationsAsync(null, null, null);
                vm.ConfiguredUsers = configs.Where(c => c.IsConfigured).ToList();
                return View(vm);
            }
        }

        // ── Generate Bulk POST ─────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateBulk(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                TempData["Error"] = "To date must be on or after From date.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _batchRepo.GenerateBulkAsync(fromDate, toDate, CurrentUserId);

            if (result.SuccessCount > 0)
            {
                var msg = $"Bulk generation complete: {result.SuccessCount} batch(es) created for {string.Join(", ", result.GeneratedUsers)}.";
                if (result.SkippedCount > 0)
                    msg += $" {result.SkippedCount} skipped (no eligible payments).";
                TempData["Success"] = msg;
            }
            else if (result.SkippedCount > 0)
            {
                TempData["Error"] = $"No batches generated. {result.SkippedCount} user(s) had no eligible payments in the selected period.";
            }
            else
            {
                TempData["Error"] = "No configured users found or all failed.";
            }

            if (result.Errors.Any())
                TempData["Error"] = string.Join(" | ", result.Errors);

            return RedirectToAction(nameof(Index));
        }

        // ── Preview (AJAX) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(int userId, DateTime fromDate, DateTime toDate)
        {
            if (userId <= 0 || fromDate > toDate)
                return Json(new { success = false, message = "Invalid parameters." });

            var result = await _batchRepo.PreviewEligiblePaymentsAsync(userId, fromDate, toDate);
            if (result == null)
                return Json(new { success = false, message = "No sales commission configuration found for this user." });

            return Json(new { success = true, data = result });
        }

        // ── Details ────────────────────────────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var batch = await _batchRepo.GetBatchByIdAsync(id);
            if (batch == null) return NotFound();

            var lines = await _batchRepo.GetBatchLinesAsync(id);
            var history = await _batchRepo.GetApprovalHistoryAsync(id);

            var currentUserId = CurrentUserId;
            var currentUser = await _userRepo.GetByIdAsync(currentUserId);
            var isAdmin = User.IsInRole("Administrator");
            var isCoreMember = currentUser?.IsCoreMember ?? false;

            var vm = new SalesCommBatchDetailsViewModel
            {
                Batch = batch,
                Lines = lines.ToList(),
                ApprovalHistory = history.ToList(),
                CanSubmitForApproval = batch.Status == "Draft" && isAdmin,
                CanApproveL1 = batch.Status == "PendingApproval" && isCoreMember,
                CanApproveL2 = batch.Status == "L1Approved" && isAdmin,
                CanSettle = batch.Status == "Approved" && isAdmin,
                CanDelete = batch.Status == "Draft" && isAdmin
            };
            return View(vm);
        }

        // ── Submit for Approval ────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForApproval(int id)
        {
            var ok = await _batchRepo.SubmitForApprovalAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Batch submitted for approval."
                : "Failed to submit. Only Draft batches can be submitted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Approve / Reject ───────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int level, string status, string? remarks)
        {
            if (status != "Approved" && status != "Rejected")
                return BadRequest();

            try
            {
                var ok = await _batchRepo.ApproveOrRejectAsync(id, level, CurrentUserId, status, remarks);
                if (ok)
                    TempData["Success"] = status == "Approved" ? "Batch approved successfully." : "Batch rejected.";
                else
                    TempData["Error"] = "Action failed. The batch may have already been processed.";
            }
            catch (Exception)
            {
                TempData["Error"] = "A connection error occurred. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ── Approval Inbox ─────────────────────────────────────────
        public async Task<IActionResult> ApprovalInbox()
        {
            var currentUser = await _userRepo.GetByIdAsync(CurrentUserId);
            var isAdmin = User.IsInRole("Administrator");
            var isCoreMember = currentUser?.IsCoreMember ?? false;

            var pending = new List<SalesCommissionBatch>();
            int level = 0;

            if (isCoreMember)
            {
                level = 1;
                var l1 = await _batchRepo.GetPendingApprovalsAsync(1, CurrentUserId);
                pending.AddRange(l1);
            }

            if (isAdmin)
            {
                level = level == 1 ? 1 : 2;
                var l2 = await _batchRepo.GetPendingApprovalsAsync(2, null);
                var existingIds = pending.Select(p => p.Id).ToHashSet();
                pending.AddRange(l2.Where(b => !existingIds.Contains(b.Id)));
            }

            var vm = new SalesCommApprovalInboxViewModel
            {
                PendingBatches = pending,
                ApproverLevel = level
            };
            return View(vm);
        }

        // ── Settlement Desk ────────────────────────────────────────
        public async Task<IActionResult> SettlementDesk()
        {
            var batches = await _batchRepo.GetBatchesAsync(null, "Approved", null, null);
            return View(batches.ToList());
        }

        // ── Settle GET ─────────────────────────────────────────────
        public async Task<IActionResult> Settle(int id)
        {
            var batch = await _batchRepo.GetBatchByIdAsync(id);
            if (batch == null || batch.Status != "Approved") return NotFound();

            var banks = await _bankRepo.GetAllAsync();
            var modes = await _modeRepo.GetAllActiveAsync();

            var vm = new SalesCommSettlementFormViewModel
            {
                BatchId = id,
                UserName = batch.UserName,
                NetCommission = batch.NetCommission,
                SettlementAmount = batch.NetCommission,
                Banks = banks.ToList(),
                PaymentModes = modes.ToList()
            };
            return View(vm);
        }

        // ── Settle POST ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settle(SalesCommSettlementFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var banks = await _bankRepo.GetAllAsync();
                var modes = await _modeRepo.GetAllActiveAsync();
                vm.Banks = banks.ToList();
                vm.PaymentModes = modes.ToList();
                return View(vm);
            }

            var ok = await _batchRepo.SettleBatchAsync(vm, CurrentUserId);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Settlement completed successfully."
                : "Failed to settle. The batch may no longer be in Approved status.";

            return RedirectToAction(nameof(Details), new { id = vm.BatchId });
        }

        // ── Delete Draft ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _batchRepo.DeleteBatchAsync(id);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Batch deleted."
                : "Only Draft batches can be deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

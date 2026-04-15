using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class PayoutBatchController : Controller
    {
        private readonly IPayoutBatchRepository _batchRepo;
        private readonly IPayoutConfigurationRepository _configRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBankMasterRepository _bankRepo;
        private readonly IPaymentModeRepository _modeRepo;

        public PayoutBatchController(
            IPayoutBatchRepository batchRepo,
            IPayoutConfigurationRepository configRepo,
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

        // ── Index: list all batches ────────────────────────────────
        public async Task<IActionResult> Index(int? userId, string? status, DateTime? from, DateTime? to)
        {
            var batches = await _batchRepo.GetBatchesAsync(userId, status, from, to);
            var users = await _userRepo.GetEmployeesAsync();

            var vm = new PayoutBatchIndexViewModel
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

        // ── Generate GET: show form ────────────────────────────────
        public async Task<IActionResult> Generate()
        {
            var configuredUsers = await _configRepo.GetAllConfigurationsAsync(null, null, "Configured");
            var vm = new PayoutGenerateFormViewModel
            {
                ConfiguredUsers = configuredUsers.ToList(),
                FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                ToDate = DateTime.Today
            };
            return View(vm);
        }

        // ── Generate POST: create batch ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(PayoutGenerateFormViewModel vm)
        {
            if (vm.FromDate > vm.ToDate)
                ModelState.AddModelError(nameof(vm.ToDate), "To date must be on or after From date.");

            if (!ModelState.IsValid)
            {
                var configuredUsers = await _configRepo.GetAllConfigurationsAsync(null, null, "Configured");
                vm.ConfiguredUsers = configuredUsers.ToList();
                return View(vm);
            }

            try
            {
                var batchId = await _batchRepo.GenerateBatchAsync(vm.UserId, vm.FromDate, vm.ToDate, null, CurrentUserId);
                TempData["Success"] = "Payout batch generated successfully.";
                return RedirectToAction(nameof(Details), new { id = batchId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                var configuredUsers = await _configRepo.GetAllConfigurationsAsync(null, null, "Configured");
                vm.ConfiguredUsers = configuredUsers.ToList();
                return View(vm);
            }
        }

        // ── Preview (AJAX) ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(int userId, DateTime fromDate, DateTime toDate)
        {
            if (userId <= 0 || fromDate > toDate)
                return Json(new { success = false, message = "Invalid parameters." });

            var result = await _batchRepo.PreviewEligibleTasksAsync(userId, fromDate, toDate);
            if (result == null)
                return Json(new { success = false, message = "No payout configuration found for this user." });

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

            var vm = new PayoutBatchDetailsViewModel
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

            // Determine approver level: core members see L1, admins see both
            var pending = new List<PayoutBatch>();
            int level = 0;

            if (isCoreMember)
            {
                level = 1;
                var l1 = await _batchRepo.GetPendingApprovalsAsync(1, CurrentUserId);
                pending.AddRange(l1);
            }

            if (isAdmin)
            {
                level = level == 1 ? 1 : 2; // If both core member and admin, show L1 first
                var l2 = await _batchRepo.GetPendingApprovalsAsync(2, null);
                // Avoid duplicates
                var existingIds = pending.Select(p => p.Id).ToHashSet();
                pending.AddRange(l2.Where(b => !existingIds.Contains(b.Id)));
            }

            var vm = new PayoutApprovalInboxViewModel
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

            var vm = new PayoutSettlementFormViewModel
            {
                BatchId = id,
                UserName = batch.UserName,
                NetAmount = batch.NetAmount,
                SettlementAmount = batch.NetAmount,
                Banks = banks.ToList(),
                PaymentModes = modes.ToList()
            };
            return View(vm);
        }

        // ── Settle POST ────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settle(PayoutSettlementFormViewModel vm)
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

        // ── Delete Draft Batch ─────────────────────────────────────
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

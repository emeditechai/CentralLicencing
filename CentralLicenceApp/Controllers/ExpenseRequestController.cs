using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CentralLicenceApp.Services;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class ExpenseRequestController : Controller
    {
        private readonly IExpenseRequestRepository _requestRepo;
        private readonly IUserRepository _userRepo;
        private readonly IExpenseCategoryRepository _expenseCategoryRepo;
        private readonly ICompanySettingsRepository _companySettingsRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<ExpenseRequestController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExpenseRequestController(IExpenseRequestRepository requestRepo, IUserRepository userRepo, IExpenseCategoryRepository expenseCategoryRepo, ICompanySettingsRepository companySettingsRepo, IEmailService emailService, ILogger<ExpenseRequestController> logger, IWebHostEnvironment environment)
        {
            _requestRepo = requestRepo;
            _userRepo = userRepo;
            _expenseCategoryRepo = expenseCategoryRepo;
            _companySettingsRepo = companySettingsRepo;
            _emailService = emailService;
            _logger = logger;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();

            var requests = await _requestRepo.GetRequestsForEmployeeAsync(currentUser.Id);
            return View(new ExpenseRequestListPageViewModel
            {
                Requests = requests.ToList(),
                PageTitle = "Expense & Advance Requests",
                EmptyStateTitle = "No requests created yet",
                EmptyStateDescription = "Create a draft request, add expense or advance booking items, then submit it for approval.",
                ShowCreateButton = true
            });
        }

        public async Task<IActionResult> Approvals()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Challenge();

            var requests = HasSuperAdminAccess() || User.IsInRole("Administrator")
                ? await _requestRepo.GetAllAsync()
                : await _requestRepo.GetPendingApprovalsAsync(currentUser.Id);

            var filtered = HasSuperAdminAccess() || User.IsInRole("Administrator")
                ? requests.Where(r => r.Status == ExpenseRequestStatus.PendingApproval).ToList()
                : requests.ToList();

            return View(new ExpenseRequestListPageViewModel
            {
                Requests = filtered,
                PageTitle = "Approval Inbox",
                EmptyStateTitle = "No approvals pending",
                EmptyStateDescription = "Requests awaiting your action will appear here.",
                ShowCreateButton = false
            });
        }

        public async Task<IActionResult> FinanceDesk()
        {
            var currentUser = await GetCurrentUserAsync();
            if (!HasFinanceAccess(currentUser)) return RedirectToAction("AccessDenied", "Account");

            var requests = await _requestRepo.GetFinanceQueueAsync();
            return View("Index", new ExpenseRequestListPageViewModel
            {
                Requests = requests.ToList(),
                PageTitle = "Reimbursement & Settlement Desk",
                EmptyStateTitle = "No approved requests ready for finance processing",
                EmptyStateDescription = "Approved requests will appear here for reimbursement and settlement.",
                ShowCreateButton = false,
                ShowFinanceActions = true
            });
        }

        public async Task<IActionResult> Create()
        {
            var currentUser = await GetCurrentUserAsync();
            if (!CanCreateRequest(currentUser)) return RedirectToAction("AccessDenied", "Account");
            return View(new ExpenseRequestFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseRequestFormViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            if (!CanCreateRequest(currentUser)) return RedirectToAction("AccessDenied", "Account");
            if (!ModelState.IsValid) return View(vm);

            var request = new ExpenseRequest
            {
                EmployeeId = currentUser!.Id,
                ApproverId = currentUser.IsCoreMember ? currentUser.Id : currentUser.ManagerId,
                PurposeOfTravel = vm.PurposeOfTravel.Trim(),
                EmployeeRemarks = string.IsNullOrWhiteSpace(vm.EmployeeRemarks) ? null : vm.EmployeeRemarks.Trim()
            };

            var requestId = await _requestRepo.CreateDraftAsync(request);
            TempData["Success"] = "Draft request created. Add expenses or advance bookings and then submit it for approval.";
            return RedirectToAction(nameof(Manage), new { id = requestId });
        }

        public async Task<IActionResult> Manage(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (currentUser == null || request == null || !CanViewRequest(currentUser, request)) return RedirectToAction("AccessDenied", "Account");

            var lines = (await _requestRepo.GetLinesAsync(id)).ToList();
            var history = (await _requestRepo.GetHistoryAsync(id)).ToList();

            return View(await BuildManageViewModel(request, lines, history, currentUser));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDraft(ExpenseRequestFormViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.Id);
            if (currentUser == null || request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            if (!ModelState.IsValid)
            {
                var lines = (await _requestRepo.GetLinesAsync(vm.Id)).ToList();
                var history = (await _requestRepo.GetHistoryAsync(vm.Id)).ToList();
                var pageVm = await BuildManageViewModel(request, lines, history, currentUser);
                pageVm.RequestForm = vm;
                return View("Manage", pageVm);
            }

            request.PurposeOfTravel = vm.PurposeOfTravel.Trim();
            request.EmployeeRemarks = string.IsNullOrWhiteSpace(vm.EmployeeRemarks) ? null : vm.EmployeeRemarks.Trim();
            await _requestRepo.UpdateDraftAsync(request);
            TempData["Success"] = "Draft request updated.";
            return RedirectToAction(nameof(Manage), new { id = vm.Id });
        }

        public async Task<IActionResult> AddLine(int requestId)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (currentUser == null || request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            var vm = await BuildLineFormAsync(new ExpenseRequestLineFormViewModel { RequestId = requestId });
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLine(ExpenseRequestLineFormViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (currentUser == null || request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            ValidateLineDates(vm);
            if (!ModelState.IsValid)
            {
                vm = await BuildLineFormAsync(vm);
                return View(vm);
            }

            var line = new ExpenseRequestLine
            {
                RequestId = vm.RequestId,
                ItemType = vm.ItemType,
                ExpenseCategoryId = vm.ExpenseCategoryId,
                Title = vm.Title.Trim(),
                ProjectOrCostCenter = vm.ProjectOrCostCenter?.Trim(),
                ExpenseDate = vm.ExpenseDate,
                CurrencyCode = vm.CurrencyCode,
                Amount = vm.Amount,
                PayableAmountInr = vm.PayableAmountInr,
                AccommodationCountry = vm.AccommodationCountry?.Trim(),
                AccommodationCity = vm.AccommodationCity?.Trim(),
                CheckInDate = vm.CheckInDate,
                CheckOutDate = vm.CheckOutDate,
                Notes = vm.Notes?.Trim(),
                ReceiptPath = null
            };

            var attachments = await SaveReceiptAttachmentsAsync(vm.ReceiptFiles);
            line.ReceiptPath = attachments.FirstOrDefault()?.FilePath;

            var lineId = await _requestRepo.AddLineAsync(line);
            await _requestRepo.AddLineAttachmentsAsync(lineId, attachments);
            TempData["Success"] = $"{vm.ItemType} line added to the request.";
            return RedirectToAction(nameof(Manage), new { id = vm.RequestId });
        }

        public async Task<IActionResult> EditLine(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var line = await _requestRepo.GetLineByIdAsync(id);
            if (currentUser == null || line == null) return RedirectToAction("AccessDenied", "Account");
            var request = await _requestRepo.GetByIdAsync(line.RequestId);
            if (request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            var vm = await BuildLineFormAsync(new ExpenseRequestLineFormViewModel
            {
                Id = line.Id,
                RequestId = line.RequestId,
                ItemType = line.ItemType,
                ExpenseCategoryId = line.ExpenseCategoryId,
                Title = line.Title,
                ProjectOrCostCenter = line.ProjectOrCostCenter,
                ExpenseDate = line.ExpenseDate,
                CurrencyCode = line.CurrencyCode,
                Amount = line.Amount,
                PayableAmountInr = line.PayableAmountInr,
                AccommodationCountry = line.AccommodationCountry,
                AccommodationCity = line.AccommodationCity,
                CheckInDate = line.CheckInDate,
                CheckOutDate = line.CheckOutDate,
                ExistingAttachments = line.Attachments.ToList(),
                Notes = line.Notes
            });
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLine(int id, ExpenseRequestLineFormViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var line = await _requestRepo.GetLineByIdAsync(id);
            if (currentUser == null || line == null || id != vm.Id) return RedirectToAction("AccessDenied", "Account");
            var request = await _requestRepo.GetByIdAsync(line.RequestId);
            if (request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            ValidateLineDates(vm);
            if (!ModelState.IsValid)
            {
                vm = await BuildLineFormAsync(vm);
                return View(vm);
            }

            line.ItemType = vm.ItemType;
            line.ExpenseCategoryId = vm.ExpenseCategoryId;
            line.Title = vm.Title.Trim();
            line.ProjectOrCostCenter = vm.ProjectOrCostCenter?.Trim();
            line.ExpenseDate = vm.ExpenseDate;
            line.CurrencyCode = vm.CurrencyCode;
            line.Amount = vm.Amount;
            line.PayableAmountInr = vm.PayableAmountInr;
            line.AccommodationCountry = vm.AccommodationCountry?.Trim();
            line.AccommodationCity = vm.AccommodationCity?.Trim();
            line.CheckInDate = vm.CheckInDate;
            line.CheckOutDate = vm.CheckOutDate;
            line.Notes = vm.Notes?.Trim();

            var newAttachments = await SaveReceiptAttachmentsAsync(vm.ReceiptFiles);
            if (newAttachments.Any() && string.IsNullOrWhiteSpace(line.ReceiptPath))
            {
                line.ReceiptPath = newAttachments.First().FilePath;
            }

            await _requestRepo.UpdateLineAsync(line);
            await _requestRepo.AddLineAttachmentsAsync(line.Id, newAttachments);
            TempData["Success"] = "Request line updated.";
            return RedirectToAction(nameof(Manage), new { id = line.RequestId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLine(int id, int requestId)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(requestId);
            if (currentUser == null || request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            await _requestRepo.DeleteLineAsync(id, requestId);
            TempData["Success"] = "Request line deleted.";
            return RedirectToAction(nameof(Manage), new { id = requestId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (currentUser == null || request == null || request.EmployeeId != currentUser.Id || request.Status != ExpenseRequestStatus.Draft)
                return RedirectToAction("AccessDenied", "Account");

            if (!currentUser.IsCoreMember && currentUser.ManagerId == null)
            {
                TempData["Error"] = "Manager is not configured for this employee. Contact administration before submitting the request.";
                return RedirectToAction(nameof(Manage), new { id });
            }

            var submitted = await _requestRepo.SubmitAsync(id, currentUser.Id, currentUser.ManagerId, currentUser.IsCoreMember, request.EmployeeRemarks);
            if (!submitted)
            {
                TempData["Error"] = "Request could not be submitted. Ensure at least one line item exists and the request is still in draft.";
                return RedirectToAction(nameof(Manage), new { id });
            }

            var submittedRequest = await _requestRepo.GetByIdAsync(id);
            if (submittedRequest != null)
            {
                await SendCoreMemberSubmissionNotificationsAsync(submittedRequest);
            }

            TempData["Success"] = currentUser.IsCoreMember
                ? "Request auto approved because the employee is marked as Core Member."
                : "Request submitted for manager approval.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (request == null || !CanViewRequest(currentUser, request))
                return RedirectToAction("AccessDenied", "Account");

            var lines = (await _requestRepo.GetLinesAsync(id)).ToList();
            var history = (await _requestRepo.GetHistoryAsync(id)).ToList();
            return View(await BuildManageViewModel(request, lines, history, currentUser));
        }

        public async Task<IActionResult> Print(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (request == null || !CanViewRequest(currentUser, request))
                return RedirectToAction("AccessDenied", "Account");

            var lines = (await _requestRepo.GetLinesAsync(id)).ToList();
            var history = (await _requestRepo.GetHistoryAsync(id)).ToList();
            var vm = await BuildManageViewModel(request, lines, history, currentUser);

            ViewBag.ParentCompany = await _companySettingsRepo.GetParentCompanyAsync();
            return View(vm);
        }

        public async Task<IActionResult> SettlementReceipt(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (request == null || request.Status != ExpenseRequestStatus.Settled || !CanViewRequest(currentUser, request))
                return RedirectToAction("AccessDenied", "Account");

            var lines = (await _requestRepo.GetLinesAsync(id)).ToList();
            var history = (await _requestRepo.GetHistoryAsync(id)).ToList();
            var vm = await BuildManageViewModel(request, lines, history, currentUser);

            ViewBag.ParentCompany = await _companySettingsRepo.GetParentCompanyAsync();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ExpenseApprovalDecisionViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (currentUser == null || request == null || !CanApproveRequest(currentUser, request))
                return RedirectToAction("AccessDenied", "Account");

            await _requestRepo.ApproveAsync(vm.RequestId, currentUser.Id, vm.Remarks?.Trim());
            TempData["Success"] = $"Request {request.RequestNumber} approved.";
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(ExpenseApprovalDecisionViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (currentUser == null || request == null || !CanApproveRequest(currentUser, request))
                return RedirectToAction("AccessDenied", "Account");

            if (string.IsNullOrWhiteSpace(vm.Remarks))
            {
                TempData["Error"] = "Remarks are required to reject a request.";
                return RedirectToAction(nameof(Details), new { id = vm.RequestId });
            }

            await _requestRepo.RejectAsync(vm.RequestId, currentUser.Id, vm.Remarks.Trim());
            TempData["Success"] = $"Request {request.RequestNumber} rejected.";
            return RedirectToAction(nameof(Approvals));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> StartReimbursement([Bind(Prefix = "ReimbursementForm")] ReimbursementStartViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (!HasFinanceAccess(currentUser) || currentUser == null || request == null)
                return RedirectToAction("AccessDenied", "Account");

            if (request.Status != ExpenseRequestStatus.Approved)
            {
                TempData["Error"] = "Only approved requests can enter reimbursement processing.";
                return RedirectToAction(nameof(Details), new { id = vm.RequestId });
            }

            if (string.IsNullOrWhiteSpace(vm.Remarks))
            {
                TempData["Error"] = "Reimbursement remarks are required.";
                return RedirectToAction(nameof(Details), new { id = vm.RequestId });
            }

            var started = await _requestRepo.StartReimbursementAsync(vm.RequestId, currentUser!.Id, vm.Remarks.Trim());
            if (!started)
            {
                TempData["Error"] = "Reimbursement could not be started for this request.";
                return RedirectToAction(nameof(Details), new { id = vm.RequestId });
            }

            TempData["Success"] = $"Reimbursement started for request {request.RequestNumber}.";
            return RedirectToAction(nameof(Details), new { id = vm.RequestId });
        }

        public async Task<IActionResult> Settle(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(id);
            if (!HasFinanceAccess(currentUser) || currentUser == null || request == null)
                return RedirectToAction("AccessDenied", "Account");

            if (request.Status != ExpenseRequestStatus.ReimbursementInProcess)
            {
                TempData["Error"] = "Only reimbursement-in-process requests can be settled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(BuildSettlementForm(request));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Settle(ExpenseSettlementViewModel vm)
        {
            var currentUser = await GetCurrentUserAsync();
            var request = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (!HasFinanceAccess(currentUser) || currentUser == null || request == null)
                return RedirectToAction("AccessDenied", "Account");

            PopulateSettlementFormOptions(vm, request);
            ValidateSettlement(vm, request);
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var settled = await _requestRepo.SettleAsync(
                vm.RequestId,
                currentUser!.Id,
                vm.SettlementDate!.Value,
                vm.SettlementAmount!.Value,
                vm.SettlementMode.Trim(),
                vm.SettlementReferenceNo.Trim(),
                string.IsNullOrWhiteSpace(vm.SettlementRemarks) ? null : vm.SettlementRemarks.Trim());

            if (!settled)
            {
                TempData["Error"] = "Settlement could not be completed. Ensure reimbursement has started for this request.";
                return View(vm);
            }

            var settledRequest = await _requestRepo.GetByIdAsync(vm.RequestId);
            if (settledRequest != null)
            {
                await SendSettlementNotificationAsync(settledRequest);
            }

            TempData["Success"] = $"Request {request.RequestNumber} settled successfully. Receipt generated.";
            return RedirectToAction(nameof(Details), new { id = vm.RequestId });
        }

        private async Task<UserMaster?> GetCurrentUserAsync()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdValue, out var userId)) return null;
            return await _userRepo.GetByIdAsync(userId);
        }

        private bool CanCreateRequest(UserMaster? currentUser)
        {
            return currentUser != null && currentUser.IsEmployee && currentUser.IsActive;
        }

        private bool CanViewRequest(UserMaster? currentUser, ExpenseRequest request)
        {
            return (currentUser != null &&
                    (request.EmployeeId == currentUser.Id
                        || request.ApproverId == currentUser.Id
                        || currentUser.RoleName == "Administrator"))
                || HasSuperAdminAccess()
                || (HasFinanceAccess(currentUser)
                    && (request.Status == ExpenseRequestStatus.Approved
                        || request.Status == ExpenseRequestStatus.ReimbursementInProcess
                        || request.Status == ExpenseRequestStatus.Settled));
        }

        private bool CanApproveRequest(UserMaster currentUser, ExpenseRequest request)
        {
            return request.Status == ExpenseRequestStatus.PendingApproval
                && (request.ApproverId == currentUser.Id
                    || currentUser.RoleName == "Administrator"
                    || HasSuperAdminAccess());
        }

        private bool HasFinanceAccess(UserMaster? currentUser)
        {
            if (HasSuperAdminAccess())
            {
                return true;
            }

            return currentUser != null
                && currentUser.IsActive
                && (currentUser.RoleName == "Finance"
                    || string.Equals(currentUser.Username, "admin", StringComparison.OrdinalIgnoreCase));
        }

        private bool HasSuperAdminAccess()
        {
            return string.Equals(User.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<ExpenseRequestManageViewModel> BuildManageViewModel(ExpenseRequest request, List<ExpenseRequestLine> lines, List<ExpenseRequestApprovalHistory> history, UserMaster? currentUser)
        {
            return new ExpenseRequestManageViewModel
            {
                Request = request,
                RequestForm = new ExpenseRequestFormViewModel
                {
                    Id = request.Id,
                    PurposeOfTravel = request.PurposeOfTravel,
                    EmployeeRemarks = request.EmployeeRemarks
                },
                NewLine = await BuildLineFormAsync(new ExpenseRequestLineFormViewModel { RequestId = request.Id }),
                ReimbursementForm = new ReimbursementStartViewModel { RequestId = request.Id },
                SettlementForm = BuildSettlementForm(request),
                Lines = lines,
                History = history,
                CanEdit = request.EmployeeId == currentUser?.Id && request.Status == ExpenseRequestStatus.Draft,
                CanSubmit = request.EmployeeId == currentUser?.Id && request.Status == ExpenseRequestStatus.Draft && lines.Any(),
                IsEmployeeOwner = request.EmployeeId == currentUser?.Id,
                CanStartReimbursement = HasFinanceAccess(currentUser) && request.Status == ExpenseRequestStatus.Approved,
                CanSettle = HasFinanceAccess(currentUser) && request.Status == ExpenseRequestStatus.ReimbursementInProcess,
                CanViewSettlementReceipt = request.Status == ExpenseRequestStatus.Settled
            };
        }

        private ExpenseSettlementViewModel BuildSettlementForm(ExpenseRequest request)
        {
            var vm = new ExpenseSettlementViewModel
            {
                RequestId = request.Id,
                RequestNumber = request.RequestNumber,
                RequestTotalAmount = request.TotalAmount,
                SettlementDate = request.SettlementDate ?? DateTime.Today,
                SettlementAmount = request.SettlementAmount ?? request.TotalAmount,
                SettlementMode = request.SettlementMode ?? string.Empty,
                SettlementReferenceNo = request.SettlementReferenceNo ?? string.Empty,
                SettlementRemarks = request.SettlementRemarks
            };

            PopulateSettlementFormOptions(vm, request);
            return vm;
        }

        private static void PopulateSettlementFormOptions(ExpenseSettlementViewModel vm, ExpenseRequest request)
        {
            vm.RequestNumber = request.RequestNumber;
            vm.RequestTotalAmount = request.TotalAmount;
            vm.AvailablePaymentModes = new List<string> { "Bank Transfer", "UPI", "Cheque", "Cash", "Corporate Card" };
        }

        private async Task<ExpenseRequestLineFormViewModel> BuildLineFormAsync(ExpenseRequestLineFormViewModel vm)
        {
            vm.ExpenseCategories = (await _expenseCategoryRepo.GetAllActiveAsync()).ToList();
            vm.AvailableCurrencies = new List<string> { "INR", "USD", "EUR", "GBP", "AED" };
            vm.AvailableCountries = new List<string> { "India", "United States", "United Kingdom", "United Arab Emirates", "Singapore", "Germany" };
            vm.AvailableItemTypes = new List<string> { "Expense", "Advance Booking" };
            return vm;
        }

        private void ValidateLineDates(ExpenseRequestLineFormViewModel vm)
        {
            if (vm.CheckInDate.HasValue && vm.CheckOutDate.HasValue && vm.CheckOutDate.Value.Date < vm.CheckInDate.Value.Date)
                ModelState.AddModelError("CheckOutDate", "Check-out cannot be earlier than check-in.");
        }

        private void ValidateSettlement(ExpenseSettlementViewModel vm, ExpenseRequest request)
        {
            if (request.Status != ExpenseRequestStatus.ReimbursementInProcess)
            {
                ModelState.AddModelError(string.Empty, "Only reimbursement-in-process requests can be settled.");
            }

            if (!vm.SettlementDate.HasValue)
            {
                ModelState.AddModelError(nameof(vm.SettlementDate), "Settlement date is required.");
            }
            else if (vm.SettlementDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.SettlementDate), "Settlement date cannot be in the future.");
            }

            if (!vm.SettlementAmount.HasValue || vm.SettlementAmount.Value <= 0)
            {
                ModelState.AddModelError(nameof(vm.SettlementAmount), "Settlement amount must be greater than zero.");
            }
            else if (vm.SettlementAmount.Value > request.TotalAmount)
            {
                ModelState.AddModelError(nameof(vm.SettlementAmount), "Settlement amount cannot be greater than the approved request total.");
            }

            if (string.IsNullOrWhiteSpace(vm.SettlementMode))
            {
                ModelState.AddModelError(nameof(vm.SettlementMode), "Payment mode is required.");
            }

            if (string.IsNullOrWhiteSpace(vm.SettlementReferenceNo))
            {
                ModelState.AddModelError(nameof(vm.SettlementReferenceNo), "Payment reference is required.");
            }

            if (vm.SettlementAmount.HasValue && vm.SettlementAmount.Value != request.TotalAmount && string.IsNullOrWhiteSpace(vm.SettlementRemarks))
            {
                ModelState.AddModelError(nameof(vm.SettlementRemarks), "Settlement remarks are required when the settled amount differs from the approved total.");
            }
        }

        private async Task<List<ExpenseRequestLineAttachment>> SaveReceiptAttachmentsAsync(IEnumerable<IFormFile>? files)
        {
            var attachments = new List<ExpenseRequestLineAttachment>();
            if (files == null)
            {
                return attachments;
            }

            foreach (var file in files)
            {
                var savedPath = await SaveReceiptAsync(file);
                if (string.IsNullOrWhiteSpace(savedPath))
                {
                    continue;
                }

                attachments.Add(new ExpenseRequestLineAttachment
                {
                    FilePath = savedPath,
                    OriginalFileName = file.FileName,
                    CreatedAt = DateTime.Now
                });
            }

            return attachments;
        }

        private async Task<string?> SaveReceiptAsync(Microsoft.AspNetCore.Http.IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".pdf", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return null;

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "expense-receipts");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream);

            return $"/uploads/expense-receipts/{fileName}";
        }

        private async Task SendCoreMemberSubmissionNotificationsAsync(ExpenseRequest request)
        {
            var coreMembers = (await _userRepo.GetCoreMembersAsync())
                .Where(user => !string.IsNullOrWhiteSpace(user.Email))
                .GroupBy(user => user.Email.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            if (coreMembers.Count == 0)
            {
                return;
            }

            var detailsUrl = Url.Action(nameof(Details), "ExpenseRequest", new { id = request.Id }, Request.Scheme) ?? string.Empty;
            var placeholders = new Dictionary<string, string>
            {
                ["RequestNumber"] = request.RequestNumber,
                ["EmployeeName"] = GetDisplayValue(request.EmployeeName),
                ["EmployeeCode"] = GetDisplayValue(request.EmployeeCode),
                ["PurposeOfTravel"] = GetDisplayValue(request.PurposeOfTravel),
                ["TotalAmount"] = request.TotalAmount.ToString("N2"),
                ["ItemCount"] = request.ItemCount.ToString(),
                ["SubmittedAt"] = (request.SubmittedAt ?? DateTime.Now).ToString("dd MMM yyyy hh:mm tt"),
                ["CurrentStatus"] = GetDisplayValue(request.Status),
                ["ApproverName"] = GetDisplayValue(request.ApproverName),
                ["DetailsUrl"] = detailsUrl
            };

            foreach (var recipient in coreMembers)
            {
                try
                {
                    await _emailService.SendTemplatedAsync(
                        "EXPENSE_REQUEST_SUBMITTED",
                        recipient.Email,
                        string.IsNullOrWhiteSpace(recipient.FullName) ? recipient.Username : recipient.FullName,
                        placeholders);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send expense submission notification for request {RequestId} to core member {Email}",
                        request.Id,
                        recipient.Email);
                }
            }
        }

        private async Task SendSettlementNotificationAsync(ExpenseRequest request)
        {
            var employee = await _userRepo.GetByIdAsync(request.EmployeeId);
            if (employee == null || string.IsNullOrWhiteSpace(employee.Email))
            {
                return;
            }

            var receiptUrl = Url.Action(nameof(SettlementReceipt), "ExpenseRequest", new { id = request.Id }, Request.Scheme) ?? string.Empty;
            var placeholders = new Dictionary<string, string>
            {
                ["RequestNumber"] = request.RequestNumber,
                ["EmployeeName"] = GetDisplayValue(request.EmployeeName),
                ["PurposeOfTravel"] = GetDisplayValue(request.PurposeOfTravel),
                ["SettlementReceiptNumber"] = GetDisplayValue(request.SettlementReceiptNumber),
                ["SettlementDate"] = request.SettlementDate?.ToString("dd MMM yyyy") ?? DateTime.Today.ToString("dd MMM yyyy"),
                ["SettlementAmount"] = (request.SettlementAmount ?? 0).ToString("N2"),
                ["SettlementMode"] = GetDisplayValue(request.SettlementMode),
                ["SettlementReferenceNo"] = GetDisplayValue(request.SettlementReferenceNo),
                ["SettlementReceiptUrl"] = receiptUrl
            };

            try
            {
                await _emailService.SendTemplatedAsync(
                    "EXPENSE_SETTLEMENT_COMPLETED",
                    employee.Email,
                    string.IsNullOrWhiteSpace(employee.FullName) ? employee.Username : employee.FullName,
                    placeholders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send settlement notification for request {RequestId} to employee {Email}",
                    request.Id,
                    employee.Email);
            }
        }

        private static string GetDisplayValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();
        }
    }
}
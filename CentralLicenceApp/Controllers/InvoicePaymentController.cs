using System;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class InvoicePaymentController : Controller
    {
        private readonly IInvoicePaymentRepository _paymentRepo;
        private readonly IInvoiceRepository        _invoiceRepo;
        private readonly IPaymentModeRepository    _modeRepo;
        private readonly IBankMasterRepository     _bankRepo;

        public InvoicePaymentController(
            IInvoicePaymentRepository paymentRepo,
            IInvoiceRepository        invoiceRepo,
            IPaymentModeRepository    modeRepo,
            IBankMasterRepository     bankRepo)
        {
            _paymentRepo = paymentRepo;
            _invoiceRepo = invoiceRepo;
            _modeRepo    = modeRepo;
            _bankRepo    = bankRepo;
        }

        // GET /InvoicePayment
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentRepo.GetAllAsync();
            return View(payments.ToList());
        }

        // GET /InvoicePayment/Create?invoiceId=5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(int? invoiceId)
        {
            var modes = (await _modeRepo.GetAllActiveAsync()).ToList();
            ViewBag.Banks = (await _bankRepo.GetAllAsync()).ToList();

            var vm = new PaymentFormViewModel
            {
                PaymentDate = DateTime.Today,
                Lines       = new() { new PaymentLineViewModel() }
            };

            if (invoiceId.HasValue)
            {
                var inv = await _invoiceRepo.GetByIdAsync(invoiceId.Value);
                if (inv == null) return NotFound();

                if (inv.Status is "Paid" or "Cancelled")
                {
                    TempData["Error"] = inv.Status == "Paid"
                        ? "This invoice is already fully paid."
                        : "Payments cannot be recorded against a cancelled invoice.";
                    return RedirectToAction("Details", "Invoice", new { id = invoiceId });
                }

                vm.InvoiceId          = inv.Id;
                vm.InvoiceNo          = inv.InvoiceNo;
                vm.PartyId            = inv.PartyId;
                vm.PartyName          = inv.PartyName;
                vm.InvoiceTotalAmount = inv.TotalAmount;
                vm.AlreadyPaid        = inv.ReceivedAmount;
                // Live calculation: actual outstanding on other open invoices for this party
                vm.PreviousBalance    = await _invoiceRepo.GetPartyOutstandingBalanceAsync(inv.PartyId, excludeInvoiceId: inv.Id);
                vm.OutstandingBalance = inv.TotalAmount - inv.ReceivedAmount;
            }

            // All open invoices for the dropdown (exclude only Paid and Cancelled)
            var allInvoices = await _invoiceRepo.GetAllAsync();
            ViewBag.UnpaidInvoices = allInvoices
                .Where(i => i.Status is not "Paid" and not "Cancelled")
                .OrderByDescending(i => i.InvoiceDate)
                .ToList();

            ViewBag.PaymentModes = modes;
            return View(vm);
        }

        // POST /InvoicePayment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(PaymentFormViewModel vm)
        {
            // Re-populate display data for re-render on error
            async Task RepopulateAsync()
            {
                var modes    = (await _modeRepo.GetAllActiveAsync()).ToList();
                ViewBag.PaymentModes = modes;
                ViewBag.Banks = (await _bankRepo.GetAllAsync()).ToList();
                var allInvoices = await _invoiceRepo.GetAllAsync();
                ViewBag.UnpaidInvoices = allInvoices
                    .Where(i => i.Status is not "Paid" and not "Cancelled")
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToList();

                if (vm.InvoiceId > 0)
                {
                    var inv = await _invoiceRepo.GetByIdAsync(vm.InvoiceId);
                    if (inv != null)
                    {
                        vm.InvoiceTotalAmount = inv.TotalAmount;
                        vm.AlreadyPaid        = inv.ReceivedAmount;
                        vm.PreviousBalance    = await _invoiceRepo.GetPartyOutstandingBalanceAsync(inv.PartyId, excludeInvoiceId: inv.Id);
                        vm.OutstandingBalance = inv.TotalAmount - inv.ReceivedAmount;
                    }
                }
            }

            if (vm.InvoiceId <= 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an invoice.");
                await RepopulateAsync();
                return View(vm);
            }

            var validLines = vm.Lines?.Where(l => l.Amount > 0).ToList();
            if (validLines == null || !validLines.Any())
            {
                ModelState.AddModelError(string.Empty, "At least one payment line with an amount is required.");
                await RepopulateAsync();
                return View(vm);
            }

            var invoice = await _invoiceRepo.GetByIdAsync(vm.InvoiceId);
            if (invoice == null) return NotFound();

            // Validate total paying does not exceed outstanding balance
            var outstanding = invoice.TotalAmount - invoice.ReceivedAmount;
            var totalPaying = validLines.Sum(l => l.Amount);
            if (totalPaying > outstanding + 0.005m)
            {
                ModelState.AddModelError(string.Empty,
                    $"Total amount being paid (₹{totalPaying:N2}) exceeds the invoice outstanding balance (₹{outstanding:N2}).");
                await RepopulateAsync();
                return View(vm);
            }

            if (invoice.Status is "Paid" or "Cancelled")
            {
                TempData["Error"] = invoice.Status == "Paid"
                    ? "This invoice is already fully paid."
                    : "Payments cannot be recorded against a cancelled invoice.";
                return RedirectToAction(nameof(Create));
            }

            // Populate PaymentModeName + bank name for each line
            var modes = (await _modeRepo.GetAllActiveAsync()).ToList();
            var banks = (await _bankRepo.GetAllAsync()).ToList();
            foreach (var line in validLines)
            {
                var mode = modes.FirstOrDefault(m => m.Id == line.PaymentModeId);
                line.PaymentModeName = mode?.Name ?? string.Empty;
            }

            var totalPaid = validLines.Sum(l => l.Amount);
            var receiptNo = await _paymentRepo.GetNextReceiptNoAsync();

            var payment = new InvoicePayment
            {
                ReceiptNo       = receiptNo,
                InvoiceId       = vm.InvoiceId,
                InvoiceNo       = invoice.InvoiceNo,
                PartyId         = invoice.PartyId,
                PartyName       = invoice.PartyName,
                PaymentDate     = vm.PaymentDate,
                TotalAmountPaid = totalPaid,
                Notes           = vm.Notes?.Trim(),
                CreatedBy       = User.Identity?.Name,
                Lines           = validLines.Select(l => new InvoicePaymentLine
                {
                    PaymentModeId   = l.PaymentModeId,
                    PaymentModeName = l.PaymentModeName,
                    Amount          = l.Amount,
                    ReferenceNo     = l.ReferenceNo?.Trim(),
                    CardType        = l.CardType?.Trim(),
                    CardLastFour    = l.CardLastFour?.Trim(),
                    BankId          = l.BankId,
                    BankName        = l.BankId.HasValue
                                        ? banks.FirstOrDefault(b => b.Id == l.BankId)?.BankName
                                        : null
                }).ToList()
            };

            var paymentId = await _paymentRepo.CreateAsync(payment);
            TempData["Success"] = $"Payment recorded. Receipt No: <strong>{receiptNo}</strong>";
            return RedirectToAction(nameof(Details), new { id = paymentId });
        }

        // GET /InvoicePayment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return NotFound();

            var companySettings = await GetCompanySettingsAsync();
            ViewBag.CompanySettings = companySettings;
            return View(payment);
        }

        // GET /InvoicePayment/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return NotFound();

            var companySettings = await GetCompanySettingsAsync();
            ViewBag.CompanySettings = companySettings;
            return View(payment);
        }

        // GET /InvoicePayment/InvoiceDetailsJson?invoiceId=5
        [HttpGet]
        public async Task<IActionResult> InvoiceDetailsJson(int invoiceId)
        {
            var inv = await _invoiceRepo.GetByIdAsync(invoiceId);
            if (inv == null) return NotFound();

            // Live calculation — not the stale snapshot stored on the invoice row
            var livePreviousBalance = await _invoiceRepo.GetPartyOutstandingBalanceAsync(inv.PartyId, excludeInvoiceId: inv.Id);

            return Json(new
            {
                partyId            = inv.PartyId,
                partyName          = inv.PartyName,
                invoiceNo          = inv.InvoiceNo,
                totalAmount        = inv.TotalAmount,
                alreadyPaid        = inv.ReceivedAmount,
                previousBalance    = livePreviousBalance,
                outstandingBalance = inv.TotalAmount - inv.ReceivedAmount
            });
        }

        // -----------------------------------------------------------------------
        private async Task<CompanySetting?> GetCompanySettingsAsync()
        {
            var companyRepo = HttpContext.RequestServices
                .GetService(typeof(ICompanySettingsRepository)) as ICompanySettingsRepository;
            return companyRepo == null ? null : await companyRepo.GetParentCompanyAsync();
        }
    }
}

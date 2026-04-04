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
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IQuotationRepository _quotationRepo;
        private readonly IPartyMasterRepository _partyRepo;
        private readonly IProductRateRepository _productRateRepo;
        private readonly IUserRepository _userRepo;

        public InvoiceController(
            IInvoiceRepository invoiceRepo,
            IQuotationRepository quotationRepo,
            IPartyMasterRepository partyRepo,
            IProductRateRepository productRateRepo,
            IUserRepository userRepo)
        {
            _invoiceRepo     = invoiceRepo;
            _quotationRepo   = quotationRepo;
            _partyRepo       = partyRepo;
            _productRateRepo = productRateRepo;
            _userRepo        = userRepo;
        }

        // GET /Invoice
        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceRepo.GetAllAsync();
            return View(invoices.ToList());
        }

        // GET /Invoice/Create
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create()
        {
            var vm = new InvoiceFormViewModel
            {
                InvoiceDate = DateTime.Today,
                DueDate     = DateTime.Today.AddDays(7),
                Lines       = new() { new InvoiceLineViewModel { SNo = 1, Qty = 1 } }
            };

            ViewBag.Parties         = (await _partyRepo.GetAllActiveAsync()).ToList();
            ViewBag.CompanyGstin    = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            ViewBag.SignatoryUsers  = (await _userRepo.GetSignatoryUsersAsync()).ToList();
            return View(vm);
        }

        // GET /Invoice/CreateFromQuotation/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateFromQuotation(int id)
        {
            var q = await _quotationRepo.GetByIdAsync(id);
            if (q == null) return NotFound();

            if (q.Status != "Accepted")
            {
                TempData["Error"] = q.Status == "Sent"
                    ? "Quotation must be <strong>Accepted</strong> before converting to Invoice. It is currently Sent — please mark it as Accepted first."
                    : $"Only an <strong>Accepted</strong> quotation can be converted to Invoice. Current status: <strong>{q.Status}</strong>.";
                return RedirectToAction("Details", "Quotation", new { id });
            }

            if (q.IsConverted)
            {
                TempData["Error"] = "This quotation has already been converted to an Invoice.";
                return RedirectToAction("Details", "Quotation", new { id });
            }

            if (q.IsCancelled)
            {
                TempData["Error"] = "A cancelled quotation cannot be converted to Invoice.";
                return RedirectToAction("Details", "Quotation", new { id });
            }

            var vm = new InvoiceFormViewModel
            {
                InvoiceDate        = DateTime.Today,
                DueDate            = DateTime.Today.AddDays(7),
                QuotationId        = q.Id,
                QuotationNo        = q.QuotationNo,
                PartyId            = q.PartyId,
                PreviousBalance    = await _invoiceRepo.GetPartyOutstandingBalanceAsync(q.PartyId),
                Notes              = q.Notes,
                TermsAndConditions = q.TermsAndConditions,
                SignatoryUserIds   = q.SignatoryUserIds.ToList(),
                Lines              = q.Lines.Select(l => new InvoiceLineViewModel
                {
                    SNo             = l.SNo,
                    ItemDescription = l.ItemDescription,
                    PlanName        = l.PlanName,
                    Type            = l.Type,
                    Qty             = l.Qty,
                    Rate            = l.Rate,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount  = l.DiscountAmount,
                    Amount          = l.Amount,
                    GstPercent      = l.GstPercent,
                    CgstAmount      = l.CgstAmount,
                    SgstAmount      = l.SgstAmount,
                    IgstAmount      = l.IgstAmount
                }).ToList()
            };

            ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
            ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
            return View("Create", vm);
        }

        // POST /Invoice/Create
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(InvoiceFormViewModel vm)
        {
            // Generate a fresh invoice number at save time.
            // Must call ModelState.Remove because the hidden field posts "" (empty string),
            // which triggers the implicit [Required] on non-nullable string in .NET 8 NRT.
            vm.InvoiceNo = await _invoiceRepo.GetNextInvoiceNoAsync();
            ModelState.Remove(nameof(vm.InvoiceNo));

            vm.Lines = vm.Lines.Where(l => !string.IsNullOrWhiteSpace(l.ItemDescription)).ToList();
            if (!vm.Lines.Any())
                ModelState.AddModelError(nameof(vm.Lines), "Add at least one line item.");

            if (!ModelState.IsValid)
            {
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var companyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            var invoice = BuildInvoice(vm, party, companyGstin);
            invoice.SignatoryUserIds = vm.SignatoryUserIds.Distinct().Take(3).ToList();
            var newId = await _invoiceRepo.CreateAsync(invoice);

            if (vm.QuotationId.HasValue && vm.QuotationId.Value > 0)
            {
                await _quotationRepo.UpdateStatusAsync(vm.QuotationId.Value, "Converted");
                TempData["Success"] = $"Invoice <strong>{vm.InvoiceNo}</strong> created from Quotation <strong>{vm.QuotationNo}</strong>.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = $"Invoice <strong>{vm.InvoiceNo}</strong> created.";
            return RedirectToAction(nameof(Details), new { id = newId });
        }

        // GET /Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var inv = await _invoiceRepo.GetByIdAsync(id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        // GET /Invoice/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var inv = await _invoiceRepo.GetByIdAsync(id);
            if (inv == null) return NotFound();

            var vm = new InvoiceFormViewModel
            {
                Id                 = inv.Id,
                InvoiceNo          = inv.InvoiceNo,
                InvoiceDate        = inv.InvoiceDate,
                DueDate            = inv.DueDate,
                QuotationId        = inv.QuotationId,
                QuotationNo        = inv.QuotationNo,
                PartyId            = inv.PartyId,
                Notes              = inv.Notes,
                TermsAndConditions = inv.TermsAndConditions,
                ReceivedAmount     = inv.ReceivedAmount,
                PreviousBalance    = inv.PreviousBalance,
                SignatoryUserIds   = inv.SignatoryUserIds,
                Lines              = inv.Lines.Select(l => new InvoiceLineViewModel
                {
                    SNo             = l.SNo,
                    ItemDescription = l.ItemDescription,
                    PlanName        = l.PlanName,
                    Type            = l.Type,
                    Qty             = l.Qty,
                    Rate            = l.Rate,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount  = l.DiscountAmount,
                    Amount          = l.Amount,
                    GstPercent      = l.GstPercent,
                    CgstAmount      = l.CgstAmount,
                    SgstAmount      = l.SgstAmount,
                    IgstAmount      = l.IgstAmount
                }).ToList()
            };

            ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
            ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
            return View(vm);
        }

        // POST /Invoice/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, InvoiceFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            vm.Lines = vm.Lines.Where(l => !string.IsNullOrWhiteSpace(l.ItemDescription)).ToList();
            if (!vm.Lines.Any())
                ModelState.AddModelError(nameof(vm.Lines), "Add at least one line item.");

            if (!ModelState.IsValid)
            {
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var companyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            var invoice = BuildInvoice(vm, party, companyGstin);
            invoice.SignatoryUserIds = vm.SignatoryUserIds.Distinct().Take(3).ToList();
            await _invoiceRepo.UpdateAsync(invoice);
            TempData["Success"] = "Invoice updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Invoice/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var inv = await _invoiceRepo.GetByIdAsync(id);
            if (inv == null) return NotFound();
            return View(inv);
        }

        // POST /Invoice/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            await _invoiceRepo.UpdateStatusAsync(id, status);
            TempData["Success"] = $"Invoice status updated to <strong>{status}</strong>.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /Invoice/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Cancel(int id, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["Error"] = "Cancellation remarks are required.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var cancelled = await _invoiceRepo.CancelAsync(id, remarks.Trim());
            TempData[cancelled ? "Success" : "Error"] = cancelled
                ? "Invoice has been cancelled."
                : "Invoice could not be cancelled (already cancelled or not found).";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Invoice/ProductRatesJson
        [HttpGet]
        public async Task<IActionResult> ProductRatesJson()
        {
            var rates = (await _productRateRepo.GetAllAsync())
                .Where(r => r.IsActive)
                .Select(r => new
                {
                    id               = r.Id,
                    productName      = r.ProductName,
                    pricingModel     = r.PricingModel,
                    productType      = r.ProductType,
                    billingModel     = r.BillingModel,
                    billingFrequency = r.BillingFrequency,
                    rate             = r.Rate
                });
            return Json(rates);
        }

        // GET /Invoice/PartyBalanceJson?partyId=5[&excludeInvoiceId=3]
        [HttpGet]
        public async Task<IActionResult> PartyBalanceJson(int partyId, int? excludeInvoiceId = null)
        {
            if (partyId <= 0) return Json(new { balance = 0m });
            var balance = await _invoiceRepo.GetPartyOutstandingBalanceAsync(partyId, excludeInvoiceId);
            return Json(new { balance });
        }

        // ─── Helpers ────────────────────────────────────────────────────
        private static Invoice BuildInvoice(InvoiceFormViewModel vm, PartyMaster party, string companyGstin)
        {
            bool isInter = (party.GSTINNo ?? string.Empty).Length >= 2 && companyGstin.Length >= 2
                           && !(party.GSTINNo!.Substring(0, 2)
                                  .Equals(companyGstin.Substring(0, 2), StringComparison.OrdinalIgnoreCase));

            var lines = new System.Collections.Generic.List<InvoiceLine>();
            int sno = 1;
            foreach (var v in vm.Lines)
            {
                var gross   = v.Qty * v.Rate;
                var discAmt = Math.Round(gross * v.DiscountPercent / 100, 2);
                var amount  = Math.Round(gross - discAmt, 2);
                var gstAmt  = Math.Round(amount * v.GstPercent / 100, 2);
                var cgst    = isInter ? 0 : Math.Round(gstAmt / 2, 2);
                var sgst    = isInter ? 0 : Math.Round(gstAmt / 2, 2);
                var igst    = isInter ? gstAmt : 0;

                lines.Add(new InvoiceLine
                {
                    SNo             = sno++,
                    ItemDescription = v.ItemDescription.Trim(),
                    PlanName        = v.PlanName?.Trim(),
                    Type            = v.Type?.Trim(),
                    Qty             = v.Qty,
                    Rate            = v.Rate,
                    DiscountPercent = v.DiscountPercent,
                    DiscountAmount  = discAmt,
                    Amount          = amount,
                    GstPercent      = v.GstPercent,
                    CgstAmount      = cgst,
                    SgstAmount      = sgst,
                    IgstAmount      = igst
                });
            }

            var subTotal   = lines.Sum(l => l.Amount);
            var totalCgst  = lines.Sum(l => l.CgstAmount);
            var totalSgst  = lines.Sum(l => l.SgstAmount);
            var totalIgst  = lines.Sum(l => l.IgstAmount);
            var totalGst   = totalCgst + totalSgst + totalIgst;
            var grandTotal = subTotal + totalGst;
            var roundOff   = Math.Round(grandTotal) - grandTotal;

            return new Invoice
            {
                Id                 = vm.Id,
                InvoiceNo          = vm.InvoiceNo,
                InvoiceDate        = vm.InvoiceDate,
                DueDate            = vm.DueDate,
                QuotationId        = vm.QuotationId,
                QuotationNo        = vm.QuotationNo,
                PartyId            = party.Id,
                PartyName          = party.PartyName,
                PartyAddress       = party.Address,
                PartyGSTINNo       = party.GSTINNo,
                PartyPANNo         = party.PANNo,
                PartyContactPerson = party.ContactPerson,
                PartyMobile        = party.Mobile,
                Notes              = vm.Notes?.Trim(),
                TermsAndConditions = vm.TermsAndConditions?.Trim(),
                SubTotal           = subTotal,
                TotalCgst          = totalCgst,
                TotalSgst          = totalSgst,
                TotalIgst          = totalIgst,
                RoundOff           = Math.Round(roundOff, 2),
                TotalAmount        = Math.Round(grandTotal),
                ReceivedAmount     = vm.ReceivedAmount,
                PreviousBalance    = vm.PreviousBalance,
                Status             = "Draft",
                Lines              = lines
            };
        }

        private async Task<CompanySetting?> GetCompanySettingsAsync()
        {
            var companyRepo = HttpContext.RequestServices
                .GetService(typeof(ICompanySettingsRepository)) as ICompanySettingsRepository;
            return companyRepo == null ? null : await companyRepo.GetParentCompanyAsync();
        }
    }
}

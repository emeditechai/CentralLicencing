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
    public class QuotationController : Controller
    {
        private readonly IQuotationRepository _quotationRepo;
        private readonly IInvoiceRepository _invoiceRepo;
        private readonly IPartyMasterRepository _partyRepo;
        private readonly IProductRateRepository _productRateRepo;
        private readonly IUserRepository _userRepo;

        public QuotationController(
            IQuotationRepository quotationRepo,
            IInvoiceRepository invoiceRepo,
            IPartyMasterRepository partyRepo,
            IProductRateRepository productRateRepo,
            IUserRepository userRepo)
        {
            _quotationRepo   = quotationRepo;
            _invoiceRepo     = invoiceRepo;
            _partyRepo       = partyRepo;
            _productRateRepo = productRateRepo;
            _userRepo        = userRepo;
        }

        // GET /Quotation
        public async Task<IActionResult> Index()
        {
            var quotations = await _quotationRepo.GetAllAsync();
            return View(quotations.ToList());
        }

        // GET /Quotation/Create
        public async Task<IActionResult> Create()
        {
            var vm = new QuotationFormViewModel
            {
                QuotationNo     = await _quotationRepo.GetNextQuotationNoAsync(),
                QuotationDate   = DateTime.Today,
                ValidUntilDate  = DateTime.Today.AddDays(30),
                Lines           = new() { new QuotationLineViewModel { SNo = 1, Qty = 1 } }
            };

            ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
            ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
            return View(vm);
        }

        // POST /Quotation/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuotationFormViewModel vm)
        {
            vm.Lines = vm.Lines.Where(l => !string.IsNullOrWhiteSpace(l.ItemDescription)).ToList();

            if (!vm.Lines.Any())
                ModelState.AddModelError(nameof(vm.Lines), "Add at least one line item.");

            if (!ModelState.IsValid)
            {
                ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                return View(vm);
            }

            var companyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            var lines = BuildLines(vm.Lines, party.GSTINNo ?? string.Empty, companyGstin);
            var subTotal   = lines.Sum(l => l.Amount);
            var totalCgst  = lines.Sum(l => l.CgstAmount);
            var totalSgst  = lines.Sum(l => l.SgstAmount);
            var totalIgst  = lines.Sum(l => l.IgstAmount);
            var totalGst   = totalCgst + totalSgst + totalIgst;

            var quotation = new Quotation
            {
                QuotationNo        = vm.QuotationNo,
                QuotationDate      = vm.QuotationDate,
                ValidUntilDate     = vm.ValidUntilDate,
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
                TotalAmount        = subTotal + totalGst,
                Status             = "Draft",
                CreatedBy          = User.Identity?.Name,
                Lines              = lines,
                SignatoryUserIds   = vm.SignatoryUserIds.Distinct().Take(3).ToList()
            };

            var id = await _quotationRepo.CreateAsync(quotation);
            TempData["Success"] = $"Quotation <strong>{vm.QuotationNo}</strong> created.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Quotation/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var q = await _quotationRepo.GetByIdAsync(id);
            if (q == null) return NotFound();
            ViewBag.CompanySettings = await GetCompanySettingsAsync();
            return View(q);
        }

        // GET /Quotation/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var q = await _quotationRepo.GetByIdAsync(id);
            if (q == null) return NotFound();
            if (q.IsConverted)
            {
                TempData["Error"] = "Converted quotations cannot be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new QuotationFormViewModel
            {
                Id                 = q.Id,
                QuotationNo        = q.QuotationNo,
                QuotationDate      = q.QuotationDate,
                ValidUntilDate     = q.ValidUntilDate,
                PartyId            = q.PartyId,
                Notes              = q.Notes,
                TermsAndConditions = q.TermsAndConditions,
                Lines              = q.Lines.Select(l => new QuotationLineViewModel
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
                }).ToList(),
                SignatoryUserIds   = q.SignatoryUserIds
            };

            ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
            ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
            return View(vm);
        }

        // POST /Quotation/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, QuotationFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            vm.Lines = vm.Lines.Where(l => !string.IsNullOrWhiteSpace(l.ItemDescription)).ToList();
            if (!vm.Lines.Any())
                ModelState.AddModelError(nameof(vm.Lines), "Add at least one line item.");

            if (!ModelState.IsValid)
            {
                ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                return View(vm);
            }

            var companyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            var lines = BuildLines(vm.Lines, party.GSTINNo ?? string.Empty, companyGstin);
            var subTotal   = lines.Sum(l => l.Amount);
            var totalCgst  = lines.Sum(l => l.CgstAmount);
            var totalSgst  = lines.Sum(l => l.SgstAmount);
            var totalIgst  = lines.Sum(l => l.IgstAmount);
            var totalGst   = totalCgst + totalSgst + totalIgst;

            var quotation = new Quotation
            {
                Id                 = vm.Id,
                QuotationNo        = vm.QuotationNo,
                QuotationDate      = vm.QuotationDate,
                ValidUntilDate     = vm.ValidUntilDate,
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
                TotalAmount        = subTotal + totalGst,
                Status             = "Draft",
                Lines              = lines,
                SignatoryUserIds   = vm.SignatoryUserIds.Distinct().Take(3).ToList()
            };

            await _quotationRepo.UpdateAsync(quotation);
            TempData["Success"] = "Quotation updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Quotation/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var q = await _quotationRepo.GetByIdAsync(id);
            if (q == null) return NotFound();
            ViewBag.CompanySettings = await GetCompanySettingsAsync();
            return View(q);
        }

        // POST /Quotation/UpdateStatus
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            await _quotationRepo.UpdateStatusAsync(id, status);
            TempData["Success"] = $"Quotation status updated to <strong>{status}</strong>.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /Quotation/Cancel/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Cancel(int id, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["Error"] = "Cancellation remarks are required.";
                return RedirectToAction(nameof(Details), new { id });
            }
            var cancelled = await _quotationRepo.CancelAsync(id, remarks.Trim());
            TempData[cancelled ? "Success" : "Error"] = cancelled
                ? "Quotation has been cancelled."
                : "Quotation could not be cancelled (already cancelled or not found).";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET /Quotation/ProductRatesJson
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

        // ─── Helpers ────────────────────────────────────────────────────
        private static System.Collections.Generic.List<QuotationLine> BuildLines(
            System.Collections.Generic.List<QuotationLineViewModel> vms,
            string partyGstin,
            string companyGstin)
        {
            bool isInter = partyGstin.Length >= 2 && companyGstin.Length >= 2
                           && !partyGstin.Substring(0, 2).Equals(companyGstin.Substring(0, 2),
                                  StringComparison.OrdinalIgnoreCase);

            var lines = new System.Collections.Generic.List<QuotationLine>();
            int sno = 1;
            foreach (var v in vms)
            {
                var grossAmount = v.Qty * v.Rate;
                var discAmt    = Math.Round(grossAmount * v.DiscountPercent / 100, 2);
                var amount     = Math.Round(grossAmount - discAmt, 2);
                var gstAmt     = Math.Round(amount * v.GstPercent / 100, 2);
                var cgst       = isInter ? 0 : Math.Round(gstAmt / 2, 2);
                var sgst       = isInter ? 0 : Math.Round(gstAmt / 2, 2);
                var igst       = isInter ? gstAmt : 0;

                lines.Add(new QuotationLine
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
            return lines;
        }

        private async Task<CompanySetting?> GetCompanySettingsAsync()
        {
            var companyRepo = HttpContext.RequestServices
                .GetService(typeof(ICompanySettingsRepository)) as ICompanySettingsRepository;
            return companyRepo == null ? null : await companyRepo.GetParentCompanyAsync();
        }
    }
}

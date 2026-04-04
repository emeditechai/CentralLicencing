using System;
using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class QuotationController : Controller
    {
        private readonly IQuotationRepository _quotationRepo;
        private readonly IInvoiceRepository    _invoiceRepo;
        private readonly IPartyMasterRepository _partyRepo;
        private readonly IProductRateRepository _productRateRepo;
        private readonly IUserRepository        _userRepo;
        private readonly IEmailService          _emailService;
        private readonly IDocumentPdfService    _pdfService;
        private readonly IServiceScopeFactory   _scopeFactory;

        public QuotationController(
            IQuotationRepository quotationRepo,
            IInvoiceRepository   invoiceRepo,
            IPartyMasterRepository partyRepo,
            IProductRateRepository productRateRepo,
            IUserRepository        userRepo,
            IEmailService          emailService,
            IDocumentPdfService    pdfService,
            IServiceScopeFactory   scopeFactory)
        {
            _quotationRepo   = quotationRepo;
            _invoiceRepo     = invoiceRepo;
            _partyRepo       = partyRepo;
            _productRateRepo = productRateRepo;
            _userRepo        = userRepo;
            _emailService    = emailService;
            _pdfService      = pdfService;
            _scopeFactory    = scopeFactory;
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
                QuotationNo     = string.Empty,
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
            // QuotationNo is empty on new form (generated at save time); suppress any validation error for it.
            ModelState.Remove(nameof(vm.QuotationNo));

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
            vm.QuotationNo = await _quotationRepo.GetNextQuotationNoAsync();
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

            // When marking as Sent: fire-and-forget PDF generation + email
            if (status == "Sent")
            {
                var q = await _quotationRepo.GetByIdAsync(id);
                if (q != null)
                {
                    var party      = await _partyRepo.GetByIdAsync(q.PartyId);
                    var partyEmail = party?.Email;
                    if (!string.IsNullOrWhiteSpace(partyEmail))
                    {
                        var partyName = party!.PartyName;
                        _ = Task.Run(async () =>
                        {
                            await using var scope = _scopeFactory.CreateAsyncScope();
                            var pdf   = scope.ServiceProvider.GetRequiredService<IDocumentPdfService>();
                            var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                            try
                            {
                                var (pdfBytes, _) = await pdf.GenerateQuotationPdfAsync(q);
                                var fileName = $"Quotation_{q.QuotationNo.Replace("/", "-")}.pdf";
                                var subject  = $"Quotation {q.QuotationNo} from {q.PartyName}";
                                var body     = BuildQuotationEmailBody(q);
                                await email.SendWithAttachmentAsync(partyEmail, partyName, subject, body, pdfBytes, fileName, "Quotation");
                            }
                            catch { /* background failure */ }
                        });
                        TempData["Success"] = $"Quotation marked as <strong>Sent</strong>. Email is being delivered to <strong>{partyEmail}</strong> in background.";
                    }
                    else
                    {
                        TempData["Success"] = "Quotation status updated to <strong>Sent</strong>. No email address on file for this party.";
                    }
                }
                else
                {
                    TempData["Success"] = "Quotation status updated to <strong>Sent</strong>.";
                }
            }
            else
            {
                TempData["Success"] = $"Quotation status updated to <strong>{status}</strong>.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private static string BuildQuotationEmailBody(Quotation q)
        {
            var validInfo = q.ValidUntilDate.HasValue
                ? $"<p style='margin:0 0 6px;'>This quotation is valid until <strong>{q.ValidUntilDate.Value:dd MMM yyyy}</strong>.</p>"
                : string.Empty;
            var linesHtml = string.Join("", q.Lines.Select((l, i) =>
                $"<tr style='background:{((i % 2 == 0) ? "#f9f9fd" : "#ffffff")};'>" +
                $"<td style='padding:7px 10px;border-bottom:1px solid #eeeff6;'>{l.ItemDescription}{(!string.IsNullOrWhiteSpace(l.PlanName) ? " <span style=\"color:#6366f1;font-size:12px;\">• " + l.PlanName + "</span>" : "")}</td>" +
                $"<td style='padding:7px 10px;border-bottom:1px solid #eeeff6;text-align:center;'>{l.Qty}</td>" +
                $"<td style='padding:7px 10px;border-bottom:1px solid #eeeff6;text-align:right;'>₹{l.Rate:0.00}</td>" +
                $"<td style='padding:7px 10px;border-bottom:1px solid #eeeff6;text-align:right;font-weight:600;'>₹{l.Amount:0.00}</td>" +
                "</tr>"));

            return $@"<!DOCTYPE html><html><body style='margin:0;padding:0;background:#f1f5f9;font-family:Arial,sans-serif;'>
<table width='100%' cellpadding='0' cellspacing='0' style='background:#f1f5f9;padding:30px 0;'>
  <tr><td align='center'>
    <table width='600' cellpadding='0' cellspacing='0' style='background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.08);'>
      <!-- Header -->
      <tr><td style='background:#1e293b;padding:28px 32px;'>
        <h1 style='margin:0;color:#ffffff;font-size:22px;letter-spacing:.04em;'>Quotation {q.QuotationNo}</h1>
        <p style='margin:6px 0 0;color:#94a3b8;font-size:13px;'>Dated {q.QuotationDate:dd MMM yyyy}</p>
      </td></tr>
      <!-- Body -->
      <tr><td style='padding:28px 32px;'>
        <p style='margin:0 0 10px;font-size:15px;color:#1e293b;'>Dear <strong>{q.PartyName}</strong>,</p>
        <p style='margin:0 0 20px;font-size:14px;color:#475569;line-height:1.6;'>
          Thank you for your interest. Please find our quotation attached for your review.
        </p>
        <!-- Summary table -->
        <table width='100%' cellpadding='0' cellspacing='0' style='border-radius:8px;overflow:hidden;border:1px solid #e2e4f0;margin-bottom:20px;'>
          <thead>
            <tr style='background:#1e293b;'>
              <th style='padding:9px 10px;text-align:left;color:#e2e8f0;font-size:12px;'>Item</th>
              <th style='padding:9px 10px;text-align:center;color:#e2e8f0;font-size:12px;'>Qty</th>
              <th style='padding:9px 10px;text-align:right;color:#e2e8f0;font-size:12px;'>Rate</th>
              <th style='padding:9px 10px;text-align:right;color:#e2e8f0;font-size:12px;'>Amount</th>
            </tr>
          </thead>
          <tbody>{{linesHtml}}</tbody>
        </table>
        <!-- Totals -->
        <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
          <tr>
            <td width='60%'></td>
            <td width='40%'>
              <table width='100%' cellpadding='0' cellspacing='0' style='border-radius:8px;overflow:hidden;border:1px solid #e2e4f0;'>
                <tr><td style='padding:7px 12px;color:#475569;font-size:13px;'>Sub Total</td><td style='padding:7px 12px;text-align:right;font-size:13px;'>₹{q.SubTotal:0.00}</td></tr>
                {(q.TotalGst > 0 ? $"<tr><td style='padding:7px 12px;color:#475569;font-size:13px;'>GST</td><td style='padding:7px 12px;text-align:right;font-size:13px;'>₹{q.TotalGst:0.00}</td></tr>" : "")}
                <tr style='background:#1e293b;'><td style='padding:9px 12px;color:#ffffff;font-size:14px;font-weight:700;'>Total</td><td style='padding:9px 12px;text-align:right;color:#ffffff;font-size:14px;font-weight:700;'>₹{q.TotalAmount:0.00}</td></tr>
              </table>
            </td>
          </tr>
        </table>
        {{validInfo}}
        <p style='margin:20px 0 0;font-size:13px;color:#64748b;'>The quotation PDF is attached for your reference. Please contact us to confirm acceptance.</p>
      </td></tr>
      <!-- Footer -->
      <tr><td style='background:#f8fafc;padding:18px 32px;border-top:1px solid #e2e8f0;text-align:center;'>
        <p style='margin:0;font-size:12px;color:#94a3b8;'>This is an auto-generated email. Please do not reply directly.</p>
      </td></tr>
    </table>
  </td></tr>
</table>
</body></html>"
                .Replace("{linesHtml}", linesHtml)
                .Replace("{validInfo}", validInfo);
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

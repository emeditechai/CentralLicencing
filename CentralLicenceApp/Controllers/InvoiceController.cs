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
    public class InvoiceController : Controller
    {
        private readonly IInvoiceRepository        _invoiceRepo;
        private readonly IQuotationRepository      _quotationRepo;
        private readonly IPartyMasterRepository    _partyRepo;
        private readonly IProductRateRepository    _productRateRepo;
        private readonly IUserRepository           _userRepo;
        private readonly IInvoicePaymentRepository _paymentRepo;
        private readonly IPaymentModeRepository    _modeRepo;
        private readonly IBankMasterRepository     _bankRepo;
        private readonly IEmailService             _emailService;
        private readonly IDocumentPdfService       _pdfService;
        private readonly IServiceScopeFactory      _scopeFactory;
        private readonly ITermsConditionTemplateRepository _termsRepo;
    private readonly IFinancialYearMasterRepository _fyRepo;

        public InvoiceController(
            IInvoiceRepository        invoiceRepo,
            IQuotationRepository      quotationRepo,
            IPartyMasterRepository    partyRepo,
            IProductRateRepository    productRateRepo,
            IUserRepository           userRepo,
            IInvoicePaymentRepository paymentRepo,
            IPaymentModeRepository    modeRepo,
            IBankMasterRepository     bankRepo,
            IEmailService             emailService,
            IDocumentPdfService       pdfService,
            IServiceScopeFactory      scopeFactory,
ITermsConditionTemplateRepository termsRepo,
        IFinancialYearMasterRepository fyRepo)
    {
        _invoiceRepo     = invoiceRepo;
        _quotationRepo   = quotationRepo;
        _partyRepo       = partyRepo;
        _productRateRepo = productRateRepo;
        _userRepo        = userRepo;
        _paymentRepo     = paymentRepo;
        _modeRepo        = modeRepo;
        _bankRepo        = bankRepo;
        _emailService    = emailService;
        _pdfService      = pdfService;
        _scopeFactory    = scopeFactory;
        _termsRepo       = termsRepo;
        _fyRepo          = fyRepo;
        }

        // GET /Invoice
        public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? status)
        {
            var fromDate = (from ?? DateTime.Today.AddDays(-7)).Date;
            var toDate   = (to   ?? DateTime.Today).Date;

            var all = await _invoiceRepo.GetAllAsync();
            var invoices = all
                .Where(i => i.InvoiceDate.Date >= fromDate && i.InvoiceDate.Date <= toDate)
                .Where(i => string.IsNullOrEmpty(status) || i.Status == status)
                .ToList();

            ViewBag.From   = fromDate.ToString("yyyy-MM-dd");
            ViewBag.To     = toDate.ToString("yyyy-MM-dd");
            ViewBag.Status = status ?? string.Empty;
            return View(invoices);
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
            ViewBag.PaymentModes    = (await _modeRepo.GetAllActiveAsync()).ToList();
            ViewBag.Banks           = (await _bankRepo.GetAllAsync()).ToList();
            ViewBag.TermsTemplates  = (await _termsRepo.GetAllActiveAsync()).ToList();
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
                TempData["Error"] = $"Quotation must be <strong>Accepted</strong> before converting to Invoice. Current status: <strong>{q.Status}</strong>.";
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
                EnableRoundOff     = q.EnableRoundOff,
                Notes              = q.Notes,
                TermsConditionTemplateId = q.TermsConditionTemplateId,
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
            ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
            ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
            ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
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
                ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
                ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
                ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
                ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
                ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var companyGstin = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
            var invoice = BuildInvoice(vm, party, companyGstin);
            invoice.SignatoryUserIds = vm.SignatoryUserIds.Distinct().Take(3).ToList();
    invoice.FinancialYearId = await _fyRepo.GetCurrentFYIdAsync();
            // ReceivedAmount via "ReceivedAmount + @Paid". Start at 0 to avoid double-counting.
            if (vm.AdvancePaymentLines?.Any(l => l.Amount > 0) == true)
                invoice.ReceivedAmount = 0;

            var newId = await _invoiceRepo.CreateAsync(invoice);

            // ── Advance payment ───────────────────────────────────────────────
            // If ReceivedAmount > 0 and payment lines were collected in the modal,
            // save an InvoicePayment record immediately.
            var advanceLines = vm.AdvancePaymentLines?.Where(l => l.Amount > 0).ToList();
            if (advanceLines != null && advanceLines.Any())
            {
                var modes = (await _modeRepo.GetAllActiveAsync()).ToList();
                var banks = (await _bankRepo.GetAllAsync()).ToList();
                foreach (var l in advanceLines)
                {
                    var mode = modes.FirstOrDefault(m => m.Id == l.PaymentModeId);
                    l.PaymentModeName = mode?.Name ?? string.Empty;
                }
                var totalPaid = advanceLines.Sum(l => l.Amount);
                var receiptNo = await _paymentRepo.GetNextReceiptNoAsync();
                var savedInvoice = await _invoiceRepo.GetByIdAsync(newId);
                var payment = new InvoicePayment
                {
                    ReceiptNo       = receiptNo,
                    InvoiceId       = newId,
                    InvoiceNo       = vm.InvoiceNo,
                    PartyId         = party.Id,
                    PartyName       = party.PartyName,
                    PaymentDate     = DateTime.Today,
                    TotalAmountPaid = totalPaid,
                    Notes           = "Advance payment recorded at invoice creation.",
                    CreatedBy       = User.Identity?.Name,
                    FinancialYearId = invoice.FinancialYearId,
                    Lines           = advanceLines.Select(l => new InvoicePaymentLine
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
                await _paymentRepo.CreateAsync(payment);
                TempData["AdvanceReceipt"] = receiptNo;
            }
            // ─────────────────────────────────────────────────────────────────

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
            ViewBag.HasActivePayments = await _paymentRepo.HasActivePaymentsAsync(id);
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
                TermsConditionTemplateId = inv.TermsConditionTemplateId,
                ReceivedAmount     = inv.ReceivedAmount,
                PreviousBalance    = inv.PreviousBalance,
                SignatoryUserIds   = inv.SignatoryUserIds,
                EnableRoundOff     = inv.EnableRoundOff,
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
            ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
            ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
            ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
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
                ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
                ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
                ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
                return View(vm);
            }

            var party = await _partyRepo.GetByIdAsync(vm.PartyId);
            if (party == null)
            {
                ModelState.AddModelError(nameof(vm.PartyId), "Selected party not found.");
                ViewBag.Parties        = (await _partyRepo.GetAllActiveAsync()).ToList();
                ViewBag.CompanyGstin   = (await GetCompanySettingsAsync())?.GSTCode ?? string.Empty;
                ViewBag.SignatoryUsers = (await _userRepo.GetSignatoryUsersAsync()).ToList();
                ViewBag.PaymentModes   = (await _modeRepo.GetAllActiveAsync()).ToList();
                ViewBag.Banks          = (await _bankRepo.GetAllAsync()).ToList();
                ViewBag.TermsTemplates = (await _termsRepo.GetAllActiveAsync()).ToList();
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
            var allowed = new[] { "Draft", "Sent", "Partial" };
            if (!allowed.Contains(status))
                return BadRequest();

            await _invoiceRepo.UpdateStatusAsync(id, status);

            // When marking as Sent: fire-and-forget PDF generation + email
            if (status == "Sent")
            {
                var inv = await _invoiceRepo.GetByIdAsync(id);
                if (inv != null)
                {
                    var party      = await _partyRepo.GetByIdAsync(inv.PartyId);
                    var partyEmail = party?.Email;
                    if (!string.IsNullOrWhiteSpace(partyEmail))
                    {
                        var partyName  = party!.PartyName;
                        var capturedId = id;
                        _ = Task.Run(async () =>
                        {
                            await using var scope = _scopeFactory.CreateAsyncScope();
                            var pdf   = scope.ServiceProvider.GetRequiredService<IDocumentPdfService>();
                            var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                            try
                            {
                                var (pdfBytes, _) = await pdf.GenerateInvoicePdfAsync(inv);
                                var fileName = $"Invoice_{inv.InvoiceNo.Replace("/", "-")}.pdf";
                                var subject  = $"Invoice {inv.InvoiceNo} from {inv.PartyName}";
                                var body     = BuildInvoiceEmailBody(inv);
                                await email.SendWithAttachmentAsync(partyEmail, partyName, subject, body, pdfBytes, fileName, "Invoice");
                            }
                            catch { /* background failure — logged by email service */ }
                        });
                        TempData["Success"] = $"Invoice marked as <strong>Sent</strong>. Email is being delivered to <strong>{partyEmail}</strong> in background.";
                    }
                    else
                    {
                        TempData["Success"] = "Invoice status updated to <strong>Sent</strong>. No email address on file for this party.";
                    }
                }
                else
                {
                    TempData["Success"] = "Invoice status updated to <strong>Sent</strong>.";
                }
            }
            else
            {
                TempData["Success"] = $"Invoice status updated to <strong>{status}</strong>.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST /Invoice/ResendMail/5
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ResendMail(int id)
        {
            var inv = await _invoiceRepo.GetByIdAsync(id);
            if (inv == null) return NotFound();

            var party = await _partyRepo.GetByIdAsync(inv.PartyId);
            var partyEmail = party?.Email;
            if (string.IsNullOrWhiteSpace(partyEmail))
            {
                TempData["Warning"] = "No email address on file for this party. Mail not sent.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var partyName  = party!.PartyName;
            var capturedInv = inv;
            _ = Task.Run(async () =>
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var pdf   = scope.ServiceProvider.GetRequiredService<IDocumentPdfService>();
                var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                try
                {
                    var (pdfBytes, _) = await pdf.GenerateInvoicePdfAsync(capturedInv);
                    var fileName = $"Invoice_{capturedInv.InvoiceNo.Replace("/", "-")}.pdf";
                    var subject  = $"Invoice {capturedInv.InvoiceNo} from {capturedInv.PartyName}";
                    var body     = BuildInvoiceEmailBody(capturedInv);
                    await email.SendWithAttachmentAsync(partyEmail, partyName, subject, body, pdfBytes, fileName, "Invoice");
                }
                catch { /* background failure */ }
            });
            TempData["Success"] = $"Email is being delivered to <strong>{partyEmail}</strong> in background.";

            return RedirectToAction(nameof(Details), new { id });
        }

        private static string BuildInvoiceEmailBody(Invoice inv)
        {
            var dueInfo = inv.DueDate.HasValue
                ? $"<p style='margin:0 0 6px;'>Please ensure payment of <strong>₹{inv.CurrentBalance:0.00}</strong> is made by <strong>{inv.DueDate.Value:dd MMM yyyy}</strong>.</p>"
                : string.Empty;
            var linesHtml = string.Join("", inv.Lines.Select((l, i) =>
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
        <h1 style='margin:0;color:#ffffff;font-size:22px;letter-spacing:.04em;'>Invoice {inv.InvoiceNo}</h1>
        <p style='margin:6px 0 0;color:#94a3b8;font-size:13px;'>Dated {inv.InvoiceDate:dd MMM yyyy}</p>
      </td></tr>
      <!-- Body -->
      <tr><td style='padding:28px 32px;'>
        <p style='margin:0 0 10px;font-size:15px;color:#1e293b;'>Dear <strong>{inv.PartyName}</strong>,</p>
        <p style='margin:0 0 20px;font-size:14px;color:#475569;line-height:1.6;'>
          Please find your invoice attached. A summary is provided below for your reference.
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
          <tbody>{linesHtml}</tbody>
        </table>
        <!-- Totals -->
        <table width='100%' cellpadding='0' cellspacing='0' style='margin-bottom:20px;'>
          <tr>
            <td width='60%'></td>
            <td width='40%'>
              <table width='100%' cellpadding='0' cellspacing='0' style='border-radius:8px;overflow:hidden;border:1px solid #e2e4f0;'>
                <tr><td style='padding:7px 12px;color:#475569;font-size:13px;'>Sub Total</td><td style='padding:7px 12px;text-align:right;font-size:13px;'>₹{inv.SubTotal:0.00}</td></tr>
                {(inv.TotalGst > 0 ? $"<tr><td style='padding:7px 12px;color:#475569;font-size:13px;'>GST</td><td style='padding:7px 12px;text-align:right;font-size:13px;'>₹{inv.TotalGst:0.00}</td></tr>" : "")}
                <tr style='background:#1e293b;'><td style='padding:9px 12px;color:#ffffff;font-size:14px;font-weight:700;'>Total</td><td style='padding:9px 12px;text-align:right;color:#ffffff;font-size:14px;font-weight:700;'>₹{inv.TotalAmount:0.00}</td></tr>
                {(inv.ReceivedAmount > 0 ? $"<tr><td style='padding:7px 12px;color:#10b981;font-size:13px;'>Paid</td><td style='padding:7px 12px;text-align:right;color:#10b981;font-size:13px;'>₹{inv.ReceivedAmount:0.00}</td></tr>" : "")}
                {(inv.CurrentBalance > 0 ? $"<tr style='background:#fef3c7;'><td style='padding:8px 12px;color:#92400e;font-size:13px;font-weight:700;'>Balance Due</td><td style='padding:8px 12px;text-align:right;color:#92400e;font-size:13px;font-weight:700;'>₹{inv.CurrentBalance:0.00}</td></tr>" : "")}
              </table>
            </td>
          </tr>
        </table>
        {dueInfo}
        <p style='margin:20px 0 0;font-size:13px;color:#64748b;'>The original invoice PDF is attached to this email. Please contact us if you have any questions.</p>
      </td></tr>
      <!-- Footer -->
      <tr><td style='background:#f8fafc;padding:18px 32px;border-top:1px solid #e2e8f0;text-align:center;'>
        <p style='margin:0;font-size:12px;color:#94a3b8;'>This is an auto-generated email. Please do not reply directly.</p>
      </td></tr>
    </table>
  </td></tr>
</table>
</body></html>";
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

            // Block cancellation if there are active (non-voided) payments
            var hasPayments = await _paymentRepo.HasActivePaymentsAsync(id);
            if (hasPayments)
            {
                TempData["Error"] = "This invoice has active payment records. Please <strong>void</strong> all payments against this invoice before cancelling it.";
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
            var enableRoundOff = vm.EnableRoundOff;
            var roundOff   = enableRoundOff ? Math.Round(Math.Round(grandTotal, MidpointRounding.AwayFromZero) - grandTotal, 2) : 0m;

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
                TermsConditionTemplateId = vm.TermsConditionTemplateId,
                SubTotal           = subTotal,
                TotalCgst          = totalCgst,
                TotalSgst          = totalSgst,
                TotalIgst          = totalIgst,
                EnableRoundOff     = enableRoundOff,
                RoundOff           = roundOff,
                TotalAmount        = enableRoundOff ? Math.Round(grandTotal) : Math.Round(grandTotal, 2),
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

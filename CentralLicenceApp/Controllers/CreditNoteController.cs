using System;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class CreditNoteController : Controller
    {
        private readonly ICreditNoteRepository     _creditNoteRepo;
        private readonly IInvoiceRefundRepository  _refundRepo;
        private readonly IInvoicePaymentRepository _paymentRepo;
        private readonly IPartyMasterRepository    _partyRepo;
        private readonly IPaymentModeRepository    _modeRepo;
        private readonly ICompanySettingsRepository _companyRepo;

        public CreditNoteController(
            ICreditNoteRepository     creditNoteRepo,
            IInvoiceRefundRepository  refundRepo,
            IInvoicePaymentRepository paymentRepo,
            IPartyMasterRepository    partyRepo,
            IPaymentModeRepository    modeRepo,
            ICompanySettingsRepository companyRepo)
        {
            _creditNoteRepo = creditNoteRepo;
            _refundRepo     = refundRepo;
            _paymentRepo    = paymentRepo;
            _partyRepo      = partyRepo;
            _modeRepo       = modeRepo;
            _companyRepo    = companyRepo;
        }

        // GET /CreditNote/Generate/5  (5 = refundId)
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Generate(int id)
        {
            var refund = await _refundRepo.GetByIdAsync(id);
            if (refund == null) return NotFound();

            // Check if credit note already exists for this refund
            var existing = await _creditNoteRepo.GetByRefundIdAsync(refund.Id);
            if (existing != null)
                return RedirectToAction(nameof(Details), new { id = existing.Id });

            var payment = await _paymentRepo.GetByIdAsync(refund.PaymentId);
            if (payment == null) return NotFound();

            // Pre-fill from party master for address details
            var party = await _partyRepo.GetByIdAsync(refund.PartyId);

            var cn = new CreditNote
            {
                RefundId           = refund.Id,
                RefundNo           = refund.RefundNo,
                PaymentId          = refund.PaymentId,
                ReceiptNo          = payment.ReceiptNo,
                InvoiceId          = refund.InvoiceId,
                InvoiceNo          = refund.InvoiceNo,
                PartyId            = refund.PartyId,
                PartyName          = refund.PartyName,
                PartyAddress       = party?.Address,
                PartyGSTINNo       = party?.GSTINNo,
                PartyPANNo         = party?.PANNo,
                PartyContactPerson = party?.ContactPerson,
                PartyMobile        = party?.Mobile,
                CreditNoteDate     = refund.RefundDate,
                Amount             = refund.Amount,
                PaymentModeId      = refund.PaymentModeId,
                PaymentModeName    = refund.PaymentModeName,
                ReferenceNo        = refund.ReferenceNo,
                Reason             = refund.Remarks
            };

            ViewBag.PaymentModes = (await _modeRepo.GetAllActiveAsync()).ToList();
            ViewBag.Refund       = refund;
            return View(cn);
        }

        // POST /CreditNote/Generate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Generate(int id, CreditNote model)
        {
            var refund = await _refundRepo.GetByIdAsync(id);
            if (refund == null) return NotFound();

            // Idempotency guard
            var existing = await _creditNoteRepo.GetByRefundIdAsync(refund.Id);
            if (existing != null)
                return RedirectToAction(nameof(Details), new { id = existing.Id });

            var payment = await _paymentRepo.GetByIdAsync(refund.PaymentId);
            if (payment == null) return NotFound();

            model.CreditNoteNo     = await _creditNoteRepo.GetNextCreditNoteNoAsync();
            model.RefundId         = refund.Id;
            model.RefundNo         = refund.RefundNo;
            model.PaymentId        = refund.PaymentId;
            model.ReceiptNo        = payment.ReceiptNo;
            model.InvoiceId        = refund.InvoiceId;
            model.InvoiceNo        = refund.InvoiceNo;
            model.PartyId          = refund.PartyId;
            model.PartyName        = refund.PartyName;
            model.Amount           = refund.Amount;
            model.PaymentModeId    = refund.PaymentModeId;
            model.PaymentModeName  = refund.PaymentModeName;
            model.CreatedBy        = User.Identity?.Name;

            if (model.CreditNoteDate == default)
                model.CreditNoteDate = DateTime.Today;

            var cnId = await _creditNoteRepo.CreateAsync(model);
            TempData["Success"] = $"Credit Note <strong>{model.CreditNoteNo}</strong> issued successfully.";
            return RedirectToAction(nameof(Details), new { id = cnId });
        }

        // GET /CreditNote/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var cn = await _creditNoteRepo.GetByIdAsync(id);
            if (cn == null) return NotFound();

            ViewBag.CompanySettings = await _companyRepo.GetParentCompanyAsync();
            return View(cn);
        }

        // GET /CreditNote/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var cn = await _creditNoteRepo.GetByIdAsync(id);
            if (cn == null) return NotFound();

            ViewBag.CompanySettings = await _companyRepo.GetParentCompanyAsync();
            return View(cn);
        }
    }
}

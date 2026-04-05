using System.Collections.Generic;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class ClientLicenseController : Controller
    {
        private readonly IClientLicenseRepository _repo;
        private readonly IEmailService _emailService;
        private readonly IClientDetailsRepository _detailsRepo;
        private readonly IClientLicenseAuditLogRepository _auditRepo;

        public ClientLicenseController(
            IClientLicenseRepository repo,
            IEmailService emailService,
            IClientDetailsRepository detailsRepo,
            IClientLicenseAuditLogRepository auditRepo)
        {
            _repo         = repo;
            _emailService = emailService;
            _detailsRepo  = detailsRepo;
            _auditRepo    = auditRepo;
        }

        public async Task<IActionResult> Index(string? search, string? status, string? productType, int page = 1)
        {
            const int pageSize = 10;
            var (items, total) = await _repo.GetPagedAsync(search, status, productType, page, pageSize);
            var codesWithDetails = await _detailsRepo.GetClientCodesWithDetailsAsync();

            var vm = new ClientLicenseListViewModel
            {
                Licenses                = items.ToList(),
                SearchTerm              = search,
                StatusFilter            = status,
                ProductType             = productType,
                AvailableProductTypes   = (await _repo.GetDistinctProductTypesAsync()).ToList(),
                PageNumber              = page,
                PageSize                = pageSize,
                TotalCount              = total,
                ClientCodesWithDetails  = codesWithDetails.ToHashSet()
            };
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var license = await _repo.GetByIdAsync(id);
            if (license == null) return NotFound();

            if (!string.IsNullOrEmpty(license.ClientCode))
                ViewBag.ClientDetails = await _detailsRepo.GetByClientCodeAsync(license.ClientCode);

            return View(license);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult Create(string? productType)
        {
            return View(new ClientLicenseFormViewModel
            {
                ExpiryDate  = DateTime.Now.AddYears(1),
                ProductType = productType ?? "eRestoPOS"
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(ClientLicenseFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var license = new ClientAppLicense
            {
                ClientName        = vm.ClientName,
                ContactNumber     = vm.ContactNumber,
                EmailID           = vm.EmailID,
                AppUrl            = vm.AppUrl,
                ExpiryDate        = vm.ExpiryDate,
                AMC_Expireddate   = vm.AMC_Expireddate,
                IsActive          = vm.IsActive,
                ProductType       = vm.ProductType,
                HardDiskNumber    = string.Empty,
                ServerMacID       = string.Empty,
                MotherboardNumber = string.Empty,
                OTP_Verified      = false
            };

            await _repo.CreateAsync(license);
            TempData["Success"] = $"License for <strong>{license.ClientName}</strong> created. Key: {license.LicenseKey}";
            return RedirectToAction(nameof(Index), new { productType = license.ProductType });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id)
        {
            var license = await _repo.GetByIdAsync(id);
            if (license == null) return NotFound();

            var vm = new ClientLicenseFormViewModel
            {
                Id              = license.Id,
                ClientCode      = license.ClientCode,
                ClientName      = license.ClientName,
                ContactNumber   = license.ContactNumber,
                EmailID         = license.EmailID,
                AppUrl          = license.AppUrl,
                ExpiryDate      = license.ExpiryDate,
                AMC_Expireddate = license.AMC_Expireddate,
                IsActive        = license.IsActive,
                ProductType     = license.ProductType,
                ConnectionString = license.ConnectionString
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, ClientLicenseFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Capture before-state for email trigger detection
            var wasActive        = existing.IsActive;
            var oldExpiryDate    = existing.ExpiryDate;
            var oldAmcExpiry     = existing.AMC_Expireddate;

            existing.ClientName       = vm.ClientName;
            existing.ContactNumber    = vm.ContactNumber;
            existing.EmailID          = vm.EmailID;
            existing.AppUrl           = vm.AppUrl;
            existing.ExpiryDate       = vm.ExpiryDate;
            existing.AMC_Expireddate  = vm.AMC_Expireddate;
            existing.IsActive         = vm.IsActive;
            existing.ProductType      = vm.ProductType;
            existing.ConnectionString = vm.ConnectionString;

            await _repo.UpdateAsync(existing);

            // ── Audit log: record date field changes ──────────────────────────
            var changedBy = User.Identity?.Name ?? "System";
            if (existing.ExpiryDate.Date != oldExpiryDate.Date)
            {
                await _auditRepo.AddAsync(new ClientLicenseAuditLog
                {
                    ClientLicenseId = existing.Id,
                    ClientCode      = existing.ClientCode,
                    ClientName      = existing.ClientName,
                    ProductType     = existing.ProductType,
                    FieldChanged    = "ExpiryDate",
                    OldValue        = oldExpiryDate.ToString("dd MMM yyyy"),
                    NewValue        = existing.ExpiryDate.ToString("dd MMM yyyy"),
                    ChangedBy       = changedBy
                });
            }

            if (existing.AMC_Expireddate?.Date != oldAmcExpiry?.Date)
            {
                await _auditRepo.AddAsync(new ClientLicenseAuditLog
                {
                    ClientLicenseId = existing.Id,
                    ClientCode      = existing.ClientCode,
                    ClientName      = existing.ClientName,
                    ProductType     = existing.ProductType,
                    FieldChanged    = "AMCExpiryDate",
                    OldValue        = oldAmcExpiry?.ToString("dd MMM yyyy"),
                    NewValue        = existing.AMC_Expireddate?.ToString("dd MMM yyyy"),
                    ChangedBy       = changedBy
                });
            }
            // ─────────────────────────────────────────────────────────────────

            // ── Email notifications ────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(existing.EmailID))
            {
                var placeholders = new Dictionary<string, string>
                {
                    ["ClientName"] = existing.ClientName,
                    ["ClientCode"] = existing.ClientCode,
                    ["ExpiryDate"] = existing.ExpiryDate.ToString("dd MMM yyyy"),
                    ["AppName"]    = existing.ProductType,
                    ["AppUrl"]     = existing.AppUrl ?? string.Empty
                };

                // 1. Re-activated → Welcome Again
                if (!wasActive && existing.IsActive)
                {
                    await _emailService.SendTemplatedAsync(
                        "LICENSE_ACTIVATED", existing.EmailID, existing.ClientName, placeholders);
                }

                // 2. Expiry date extended
                if (existing.ExpiryDate > oldExpiryDate)
                {
                    await _emailService.SendTemplatedAsync(
                        "LICENSE_EXPIRY_EXTENDED", existing.EmailID, existing.ClientName, placeholders);
                }
            }

            TempData["Success"] = "License updated successfully.";
            return RedirectToAction(nameof(Index), new { productType = existing.ProductType });
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var license = await _repo.GetByIdAsync(id);
            if (license == null) return NotFound();
            return View(license);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            TempData["Success"] = "License record deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}

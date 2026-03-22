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
    public class ClientDetailsController : Controller
    {
        private readonly IClientDetailsRepository _detailsRepo;
        private readonly IClientLicenseRepository _licenseRepo;

        public ClientDetailsController(
            IClientDetailsRepository detailsRepo,
            IClientLicenseRepository licenseRepo)
        {
            _detailsRepo = detailsRepo;
            _licenseRepo = licenseRepo;
        }

        // GET: /ClientDetails/Upsert?clientCode=xxx&productType=xxx
        public async Task<IActionResult> Upsert(string clientCode, string? productType)
        {
            var license = await _licenseRepo.GetByClientCodeAsync(clientCode);
            if (license == null) return NotFound();

            var existing = await _detailsRepo.GetByClientCodeAsync(clientCode);

            var vm = new ClientDetailsViewModel
            {
                ClientCode        = clientCode,
                ClientName        = license.ClientName,
                IsActive          = true
            };

            if (existing != null)
            {
                vm.ID               = existing.ID;
                vm.ClientPersonName = existing.ClientPersonName;
                vm.Address          = existing.Address;
                vm.DOB              = existing.DOB;
                vm.Anniversarydate  = existing.Anniversarydate;
                vm.IsActive         = existing.IsActive;
                vm.SelectedProducts = string.IsNullOrWhiteSpace(existing.ProductPurchased)
                    ? new()
                    : existing.ProductPurchased.Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(p => p.Trim()).ToList();
            }

            ViewBag.ProductType = productType;
            return View(vm);
        }

        // POST: /ClientDetails/Upsert
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ClientDetailsViewModel vm, string? productType)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductType = productType;
                return View(vm);
            }

            var details = new ClientDetails
            {
                ClientCode        = vm.ClientCode,
                ClientPersonName  = vm.ClientPersonName,
                Address           = vm.Address,
                ProductPurchased  = vm.SelectedProducts.Any()
                                    ? string.Join(",", vm.SelectedProducts)
                                    : null,
                DOB               = vm.DOB,
                Anniversarydate   = vm.Anniversarydate,
                IsActive          = vm.IsActive
            };

            await _detailsRepo.UpsertAsync(details);

            TempData["Success"] = $"Client details for <strong>{vm.ClientCode}</strong> saved successfully.";
            return RedirectToAction("Index", "ClientLicense",
                string.IsNullOrWhiteSpace(productType) ? null : new { productType });
        }
    }
}

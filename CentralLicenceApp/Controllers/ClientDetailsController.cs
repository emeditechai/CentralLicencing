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
        private readonly IProductMasterRepository _productRepo;
        private readonly IProductRateRepository _productRateRepo;
        private readonly IInvoiceRepository _invoiceRepo;

        public ClientDetailsController(
            IClientDetailsRepository detailsRepo,
            IClientLicenseRepository licenseRepo,
            IProductMasterRepository productRepo,
            IProductRateRepository productRateRepo,
            IInvoiceRepository invoiceRepo)
        {
            _detailsRepo = detailsRepo;
            _licenseRepo = licenseRepo;
            _productRepo = productRepo;
            _productRateRepo = productRateRepo;
            _invoiceRepo = invoiceRepo;
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
                IsActive          = true,
                PurchasedProducts = new()
            };

            if (existing != null)
            {
                vm.ID               = existing.ID;
                vm.ClientPersonName = existing.ClientPersonName;
                vm.Address          = existing.Address;
                vm.IsInternalUse    = existing.IsInternalUse;
                vm.ReferenceClientCode = existing.ReferenceClientCode;
                vm.DOB              = existing.DOB;
                vm.Anniversarydate  = existing.Anniversarydate;
                vm.IsActive         = existing.IsActive;
                vm.PurchasedProducts = existing.PurchasedProducts.Select(p => new ClientPurchasedProductEntryViewModel
                {
                    ProductId = p.ProductId,
                    ProductRateId = p.ProductRateId,
                    ProductName = p.ProductName,
                    PricingModel = p.PricingModel,
                    BillingModel = p.BillingModel,
                    BillingFrequency = p.BillingFrequency,
                    BasePrice = p.BasePrice,
                    AmcCalculationType = p.AmcCalculationType,
                    AmcPercentage = p.AmcPercentage,
                    AmcAmount = p.AmcAmount,
                    InvoiceNo = p.InvoiceNo,
                    SubscriptionStartDate = p.SubscriptionStartDate
                }).ToList();
            }

            if (!vm.PurchasedProducts.Any())
            {
                vm.PurchasedProducts.Add(new ClientPurchasedProductEntryViewModel());
            }

            await PopulateCatalogAsync();
            ViewBag.ProductType = productType;
            return View(vm);
        }

        // POST: /ClientDetails/Upsert
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ClientDetailsViewModel vm, string? productType)
        {
            if (vm.IsInternalUse)
            {
                vm.PurchasedProducts = new();
            }
            else
            {
                vm.PurchasedProducts = vm.PurchasedProducts
                    .Where(x => x.ProductId.HasValue || x.ProductRateId.HasValue)
                    .ToList();

                await ValidatePurchasedProductsAsync(vm);
            }

            await ValidateReferenceClientCodeAsync(vm);

            if (!ModelState.IsValid)
            {
                if (!vm.IsInternalUse && !vm.PurchasedProducts.Any())
                {
                    vm.PurchasedProducts.Add(new ClientPurchasedProductEntryViewModel());
                }

                await PopulateCatalogAsync();
                ViewBag.ProductType = productType;
                return View(vm);
            }

            var purchasedProducts = new System.Collections.Generic.List<ClientPurchasedProduct>();
            foreach (var row in vm.PurchasedProducts)
            {
                var productRate = await _productRateRepo.GetByIdAsync(row.ProductRateId!.Value);
                var product = await _productRepo.GetByIdAsync(row.ProductId!.Value);

                if (productRate == null || product == null)
                {
                    continue;
                }

                purchasedProducts.Add(new ClientPurchasedProduct
                {
                    ProductId = product.Id,
                    ProductRateId = productRate.Id,
                    ProductCode = product.ProductCode,
                    ProductName = product.ProductName,
                    PricingModel = productRate.PricingModel,
                    BillingModel = productRate.BillingModel,
                    BillingFrequency = productRate.BillingFrequency,
                    BasePrice = productRate.Rate,
                    AmcCalculationType = productRate.AmcCalculationType,
                    AmcPercentage = productRate.AmcPercentage,
                    AmcAmount = productRate.AmcAmount,
                    InvoiceNo = string.IsNullOrWhiteSpace(row.InvoiceNo) ? null : row.InvoiceNo.Trim(),
                    SubscriptionStartDate = row.SubscriptionStartDate,
                    IsActive = true
                });
            }

            var details = new ClientDetails
            {
                ClientCode        = vm.ClientCode,
                ClientPersonName  = vm.ClientPersonName,
                Address           = vm.Address,
                ProductPurchased  = purchasedProducts.Any()
                                    ? string.Join(",", purchasedProducts.Select(x => x.ProductName).Distinct())
                                    : null,
                DOB               = vm.DOB,
                Anniversarydate   = vm.Anniversarydate,
                IsInternalUse     = vm.IsInternalUse,
                ReferenceClientCode = string.IsNullOrWhiteSpace(vm.ReferenceClientCode) ? null : vm.ReferenceClientCode.Trim(),
                IsActive          = vm.IsActive,
                PurchasedProducts = purchasedProducts
            };

            await _detailsRepo.UpsertAsync(details);

            TempData["Success"] = $"Client details for <strong>{vm.ClientCode}</strong> saved successfully.";
            return RedirectToAction("Index", "ClientLicense",
                string.IsNullOrWhiteSpace(productType) ? null : new { productType });
        }

        private async Task PopulateCatalogAsync()
        {
            var products = (await _productRepo.GetAllActiveAsync()).ToList();
            var productRates = (await _productRateRepo.GetAllAsync())
                .Where(x => x.IsActive)
                .ToList();

            ViewBag.ProductCatalog = products;
            ViewBag.ProductRateCatalog = productRates;
        }

        private async Task ValidatePurchasedProductsAsync(ClientDetailsViewModel vm)
        {
            if (!vm.PurchasedProducts.Any())
            {
                ModelState.AddModelError(nameof(ClientDetailsViewModel.PurchasedProducts), "Please add at least one purchased product.");
                return;
            }

            var selectedRateIds = new System.Collections.Generic.HashSet<int>();

            for (var index = 0; index < vm.PurchasedProducts.Count; index++)
            {
                var row = vm.PurchasedProducts[index];
                if (!row.ProductId.HasValue)
                {
                    ModelState.AddModelError($"PurchasedProducts[{index}].ProductId", "Product is required.");
                    continue;
                }

                if (!row.ProductRateId.HasValue)
                {
                    ModelState.AddModelError($"PurchasedProducts[{index}].ProductRateId", "Pricing Model is required.");
                    continue;
                }

                if (!selectedRateIds.Add(row.ProductRateId.Value))
                {
                    ModelState.AddModelError($"PurchasedProducts[{index}].ProductRateId", "This pricing model is already selected.");
                    continue;
                }

                var rate = await _productRateRepo.GetByIdAsync(row.ProductRateId.Value);
                if (rate == null || !rate.IsActive)
                {
                    ModelState.AddModelError($"PurchasedProducts[{index}].ProductRateId", "Please select a valid active pricing model.");
                    continue;
                }

                if (rate.ProductId != row.ProductId.Value)
                {
                    ModelState.AddModelError($"PurchasedProducts[{index}].ProductRateId", "Selected pricing model does not belong to the selected product.");
                }
            }
        }

        // GET /ClientDetails/GetInvoiceDetails?invoiceNo=EL/SL/26-27/0001
        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(string invoiceNo)
        {
            if (string.IsNullOrWhiteSpace(invoiceNo))
                return Json(new { found = false });

            var inv = await _invoiceRepo.GetByInvoiceNoAsync(invoiceNo.Trim());
            if (inv == null || inv.IsCancelled)
                return Json(new { found = false });

            return Json(new
            {
                found          = true,
                invoiceNo      = inv.InvoiceNo,
                invoiceDate    = inv.InvoiceDate.ToString("dd MMM yyyy"),
                partyName      = inv.PartyName,
                subTotal       = inv.SubTotal,
                totalGst       = inv.TotalGst,
                totalAmount    = inv.TotalAmount,
                receivedAmount = inv.ReceivedAmount,
                balanceDue     = inv.CurrentBalance,
                status         = inv.Status,
                lines          = inv.Lines.Select(l => new {
                    l.ItemDescription,
                    l.PlanName,
                    l.Type,
                    l.Qty,
                    l.Rate,
                    l.Amount
                })
            });
        }

        private async Task ValidateReferenceClientCodeAsync(ClientDetailsViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.ReferenceClientCode))
            {
                return;
            }

            var referenceCode = vm.ReferenceClientCode.Trim();
            if (string.Equals(referenceCode, vm.ClientCode, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(ClientDetailsViewModel.ReferenceClientCode), "Reference ClientCode cannot be the same as the current ClientCode.");
                return;
            }

            var referenceLicense = await _licenseRepo.GetByClientCodeAsync(referenceCode);
            if (referenceLicense == null)
            {
                ModelState.AddModelError(nameof(ClientDetailsViewModel.ReferenceClientCode), "Reference ClientCode must match an existing client.");
            }
        }
    }
}

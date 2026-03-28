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
    [Authorize(Roles = "Administrator")]
    public class ProductRateDiscountController : Controller
    {
        private static readonly string[] DiscountTypes = { "Percentage", "Flat Amount" };

        private readonly IProductRateDiscountRepository _repo;
        private readonly IProductRateRepository _productRateRepo;

        public ProductRateDiscountController(IProductRateDiscountRepository repo, IProductRateRepository productRateRepo)
        {
            _repo = repo;
            _productRateRepo = productRateRepo;
        }

        public async Task<IActionResult> Index(int? productRateId, bool todayOnly = false)
        {
            var rates = (await _productRateRepo.GetAllAsync()).ToList();
            var items = (await _repo.GetAllAsync(productRateId, todayOnly)).ToList();
            var selectedRate = productRateId.HasValue ? rates.FirstOrDefault(x => x.Id == productRateId.Value) : null;

            return View(new ProductRateDiscountIndexViewModel
            {
                ProductRates = rates,
                Items = items,
                SelectedProductRateId = productRateId,
                SelectedProductRate = selectedRate,
                TodayOnly = todayOnly
            });
        }

        public async Task<IActionResult> Create(int? productRateId)
        {
            await PopulateDropdownsAsync();
            return View(new ProductRateDiscountFormViewModel
            {
                ProductRateId = productRateId ?? 0,
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddDays(7),
                IsActive = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRateDiscountFormViewModel vm)
        {
            await PopulateDropdownsAsync();
            await ValidateOfferAsync(vm, null);
            if (!ModelState.IsValid) return View(vm);

            var offer = new ProductRateDiscountOffer
            {
                ProductRateId = vm.ProductRateId,
                DiscountName = vm.DiscountName.Trim(),
                DiscountType = vm.DiscountType.Trim(),
                DiscountValue = vm.DiscountValue,
                PromoCode = string.IsNullOrWhiteSpace(vm.PromoCode) ? null : vm.PromoCode.Trim().ToUpperInvariant(),
                ValidFrom = vm.ValidFrom,
                ValidTo = vm.ValidTo,
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(offer);
            TempData["Success"] = $"Discount offer <strong>{offer.DiscountName}</strong> created.";
            return RedirectToAction(nameof(Index), new { productRateId = offer.ProductRateId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var offer = await _repo.GetByIdAsync(id);
            if (offer == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(new ProductRateDiscountFormViewModel
            {
                Id = offer.Id,
                ProductRateId = offer.ProductRateId,
                DiscountName = offer.DiscountName,
                DiscountType = offer.DiscountType,
                DiscountValue = offer.DiscountValue,
                PromoCode = offer.PromoCode,
                ValidFrom = offer.ValidFrom,
                ValidTo = offer.ValidTo,
                Description = offer.Description,
                IsActive = offer.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductRateDiscountFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            await PopulateDropdownsAsync();
            await ValidateOfferAsync(vm, vm.Id);
            if (!ModelState.IsValid) return View(vm);

            var offer = new ProductRateDiscountOffer
            {
                Id = vm.Id,
                ProductRateId = vm.ProductRateId,
                DiscountName = vm.DiscountName.Trim(),
                DiscountType = vm.DiscountType.Trim(),
                DiscountValue = vm.DiscountValue,
                PromoCode = string.IsNullOrWhiteSpace(vm.PromoCode) ? null : vm.PromoCode.Trim().ToUpperInvariant(),
                ValidFrom = vm.ValidFrom,
                ValidTo = vm.ValidTo,
                Description = vm.Description?.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(offer);
            TempData["Success"] = "Discount offer updated.";
            return RedirectToAction(nameof(Index), new { productRateId = offer.ProductRateId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? productRateId)
        {
            var deleted = await _repo.DeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] = deleted ? "Discount offer deleted." : "The selected discount offer was not found.";
            return RedirectToAction(nameof(Index), new { productRateId });
        }

        private async Task ValidateOfferAsync(ProductRateDiscountFormViewModel vm, int? ignoreId)
        {
            var rate = await _productRateRepo.GetByIdAsync(vm.ProductRateId);
            if (rate == null)
            {
                ModelState.AddModelError(nameof(ProductRateDiscountFormViewModel.ProductRateId), "Please select a valid pricing model.");
            }

            if (string.IsNullOrWhiteSpace(vm.DiscountType) || !DiscountTypes.Contains(vm.DiscountType.Trim()))
            {
                ModelState.AddModelError(nameof(ProductRateDiscountFormViewModel.DiscountType), "Please select a valid Discount Model.");
            }

            if (vm.ValidTo.Date < vm.ValidFrom.Date)
            {
                ModelState.AddModelError(nameof(ProductRateDiscountFormViewModel.ValidTo), "Validity end date must be on or after the start date.");
            }

            if (vm.DiscountType == "Percentage" && vm.DiscountValue > 100)
            {
                ModelState.AddModelError(nameof(ProductRateDiscountFormViewModel.DiscountValue), "Percentage discount cannot exceed 100.");
            }

            if (!string.IsNullOrWhiteSpace(vm.PromoCode)
                && await _repo.PromoCodeExistsAsync(vm.PromoCode.Trim().ToUpperInvariant(), ignoreId))
            {
                ModelState.AddModelError(nameof(ProductRateDiscountFormViewModel.PromoCode), "Promo code already exists.");
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.ProductRates = (await _productRateRepo.GetAllAsync()).ToList();
            ViewBag.DiscountTypes = DiscountTypes;
        }
    }
}
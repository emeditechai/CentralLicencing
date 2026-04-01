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
    public class ProductRateController : Controller
    {
        private static readonly string[] AmcCalculationTypes = { "Percentage", "Flat Amount" };
        private static readonly string[] BillingModels = { "Subscription", "One Time" };
        private static readonly string[] BillingFrequencies = { "Monthly", "Annual", "Quarterly", "Half Yearly" };

        private readonly IProductRateRepository _repo;
        private readonly IProductMasterRepository _productRepo;
        private readonly IPricingModelRepository _pricingModelRepo;

        public ProductRateController(IProductRateRepository repo, IProductMasterRepository productRepo, IPricingModelRepository pricingModelRepo)
        {
            _repo = repo;
            _productRepo = productRepo;
            _pricingModelRepo = pricingModelRepo;
        }

        public async Task<IActionResult> Index(int? productId)
        {
            var products = (await _productRepo.GetAllAsync()).ToList();
            var items = (await _repo.GetAllAsync(productId)).ToList();

            var vm = new ProductRateIndexViewModel
            {
                Products = products,
                Items = items,
                SelectedProductId = productId,
                SelectedProduct = productId.HasValue ? products.FirstOrDefault(x => x.Id == productId.Value) : null
            };

            return View(vm);
        }

        public async Task<IActionResult> Create(int? productId)
        {
            await PopulateDropdownsAsync();
            return View(new ProductRateFormViewModel
            {
                ProductId = productId ?? 0,
                BillingModel = BillingModels[1],
                AmcCalculationType = string.Empty,
                IsActive = true
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductRateFormViewModel vm)
        {
            await PopulateDropdownsAsync();
            await ValidateRateAsync(vm, null);
            if (!ModelState.IsValid) return View(vm);

            var amc = CalculateAmc(vm.Rate, vm.AmcCalculationType, vm.AmcInputValue);
            vm.CalculatedAmcPercentage = amc.AmcPercentage;
            vm.CalculatedAmcAmount = amc.AmcAmount;

            var rate = new ProductRate
            {
                ProductId = vm.ProductId,
                PricingModel = vm.PricingModel.Trim(),
                BillingModel = vm.BillingModel.Trim(),
                BillingFrequency = NormalizeBillingFrequency(vm.BillingModel, vm.BillingFrequency),
                ProductSpecification = vm.ProductSpecification?.Trim(),
                Features = vm.Features?.Trim(),
                Rate = vm.Rate,
                AmcCalculationType = amc.AmcCalculationType,
                AmcPercentage = amc.AmcPercentage,
                AmcAmount = amc.AmcAmount,
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(rate);
            TempData["Success"] = $"Pricing model <strong>{rate.PricingModel}</strong> created.";
            return RedirectToAction(nameof(Index), new { productId = rate.ProductId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rate = await _repo.GetByIdAsync(id);
            if (rate == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(new ProductRateFormViewModel
            {
                Id = rate.Id,
                ProductId = rate.ProductId,
                PricingModel = rate.PricingModel,
                BillingModel = rate.BillingModel,
                BillingFrequency = rate.BillingFrequency,
                ProductSpecification = rate.ProductSpecification,
                Features = rate.Features,
                Rate = rate.Rate,
                AmcCalculationType = HasAmc(rate) ? rate.AmcCalculationType : string.Empty,
                AmcInputValue = HasAmc(rate)
                    ? (rate.AmcCalculationType == "Flat Amount" ? rate.AmcAmount : rate.AmcPercentage)
                    : null,
                CalculatedAmcPercentage = rate.AmcPercentage,
                CalculatedAmcAmount = rate.AmcAmount,
                IsActive = rate.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductRateFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            await PopulateDropdownsAsync();
            await ValidateRateAsync(vm, vm.Id);
            if (!ModelState.IsValid) return View(vm);

            var amc = CalculateAmc(vm.Rate, vm.AmcCalculationType, vm.AmcInputValue);
            vm.CalculatedAmcPercentage = amc.AmcPercentage;
            vm.CalculatedAmcAmount = amc.AmcAmount;

            var rate = new ProductRate
            {
                Id = vm.Id,
                ProductId = vm.ProductId,
                PricingModel = vm.PricingModel.Trim(),
                BillingModel = vm.BillingModel.Trim(),
                BillingFrequency = NormalizeBillingFrequency(vm.BillingModel, vm.BillingFrequency),
                ProductSpecification = vm.ProductSpecification?.Trim(),
                Features = vm.Features?.Trim(),
                Rate = vm.Rate,
                AmcCalculationType = amc.AmcCalculationType,
                AmcPercentage = amc.AmcPercentage,
                AmcAmount = amc.AmcAmount,
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(rate);
            TempData["Success"] = "Product rate updated.";
            return RedirectToAction(nameof(Index), new { productId = rate.ProductId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? productId)
        {
            var deleted = await _repo.DeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] = deleted ? "Product rate deleted." : "The selected product rate was not found.";
            return RedirectToAction(nameof(Index), new { productId });
        }

        private async Task ValidateRateAsync(ProductRateFormViewModel vm, int? ignoreId)
        {
            var product = await _productRepo.GetByIdAsync(vm.ProductId);
            if (product == null)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.ProductId), "Please select a valid product.");
            }

            if (string.IsNullOrWhiteSpace(vm.PricingModel)
                || !await _pricingModelRepo.ExistsActiveAsync(vm.PricingModel.Trim()))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.PricingModel), "Please select a valid Pricing Model.");
            }

            if (string.IsNullOrWhiteSpace(vm.BillingModel)
                || !BillingModels.Contains(vm.BillingModel.Trim()))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.BillingModel), "Please select a valid Model.");
            }

            if (string.Equals(vm.BillingModel?.Trim(), "Subscription", System.StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(vm.BillingFrequency)
                    || !BillingFrequencies.Contains(vm.BillingFrequency.Trim())))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.BillingFrequency), "Please select a valid Frequency for subscription pricing.");
            }

            var hasAmcType = !string.IsNullOrWhiteSpace(vm.AmcCalculationType);
            var hasAmcValue = vm.AmcInputValue.HasValue;

            if (hasAmcType && !AmcCalculationTypes.Contains(vm.AmcCalculationType!.Trim()))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcCalculationType), "Please select a valid AMC Type.");
            }

            if (vm.Rate <= 0)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.Rate), "Rate must be greater than zero.");
            }

            if (hasAmcType && !hasAmcValue)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcInputValue), "Please enter AMC value when AMC Type is selected.");
            }

            if (!hasAmcType && hasAmcValue)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcCalculationType), "Please select AMC Type when AMC Value is entered.");
            }

            if (hasAmcValue && vm.AmcInputValue <= 0)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcInputValue), "AMC value must be greater than zero.");
            }

            if (hasAmcType && hasAmcValue && vm.AmcCalculationType == "Percentage" && vm.AmcInputValue > 100)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcInputValue), "AMC percentage cannot exceed 100.");
            }

            if (product != null
                && !string.IsNullOrWhiteSpace(vm.PricingModel)
                && !string.IsNullOrWhiteSpace(vm.BillingModel)
                && await _repo.RateVariantExistsAsync(
                    vm.ProductId,
                    vm.PricingModel.Trim(),
                    vm.BillingModel.Trim(),
                    NormalizeBillingFrequency(vm.BillingModel, vm.BillingFrequency),
                    ignoreId))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.PricingModel), "This pricing model and model/frequency combination is already mapped for the selected product.");
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Products = (await _productRepo.GetAllAsync()).ToList();
            ViewBag.PricingModels = (await _pricingModelRepo.GetAllActiveAsync()).ToList();
            ViewBag.BillingModels = BillingModels;
            ViewBag.BillingFrequencies = BillingFrequencies;
            ViewBag.AmcCalculationTypes = AmcCalculationTypes;
        }

        private static string NormalizeBillingFrequency(string? billingModel, string? billingFrequency)
        {
            return string.Equals(billingModel?.Trim(), "Subscription", System.StringComparison.OrdinalIgnoreCase)
                ? (billingFrequency ?? string.Empty).Trim()
                : string.Empty;
        }

        private static ProductRate CalculateAmc(decimal rate, string? amcCalculationType, decimal? amcInputValue)
        {
            if (string.IsNullOrWhiteSpace(amcCalculationType) || !amcInputValue.HasValue)
            {
                return new ProductRate
                {
                    AmcCalculationType = string.Empty,
                    AmcPercentage = 0,
                    AmcAmount = 0
                };
            }

            var normalizedType = amcCalculationType.Trim();
            var normalizedValue = amcInputValue.Value;

            if (normalizedType == "Flat Amount")
            {
                var percentage = rate == 0 ? 0 : decimal.Round((normalizedValue / rate) * 100m, 4, System.MidpointRounding.AwayFromZero);
                return new ProductRate
                {
                    AmcCalculationType = normalizedType,
                    AmcAmount = decimal.Round(normalizedValue, 2, System.MidpointRounding.AwayFromZero),
                    AmcPercentage = percentage
                };
            }

            var amount = decimal.Round((rate * normalizedValue) / 100m, 2, System.MidpointRounding.AwayFromZero);
            return new ProductRate
            {
                AmcCalculationType = normalizedType,
                AmcPercentage = decimal.Round(normalizedValue, 4, System.MidpointRounding.AwayFromZero),
                AmcAmount = amount
            };
        }

        private static bool HasAmc(ProductRate rate)
        {
            return !string.IsNullOrWhiteSpace(rate.AmcCalculationType)
                && (rate.AmcAmount > 0 || rate.AmcPercentage > 0);
        }
    }
}
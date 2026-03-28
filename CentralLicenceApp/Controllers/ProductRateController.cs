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
                AmcCalculationType = AmcCalculationTypes[0],
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
                ProductSpecification = rate.ProductSpecification,
                Features = rate.Features,
                Rate = rate.Rate,
                AmcCalculationType = rate.AmcCalculationType,
                AmcInputValue = rate.AmcCalculationType == "Flat Amount" ? rate.AmcAmount : rate.AmcPercentage,
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

            if (string.IsNullOrWhiteSpace(vm.AmcCalculationType)
                || !AmcCalculationTypes.Contains(vm.AmcCalculationType.Trim()))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcCalculationType), "Please select a valid AMC Type.");
            }

            if (vm.Rate <= 0)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.Rate), "Rate must be greater than zero.");
            }

            if (vm.AmcInputValue <= 0)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcInputValue), "AMC value must be greater than zero.");
            }

            if (vm.AmcCalculationType == "Percentage" && vm.AmcInputValue > 100)
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.AmcInputValue), "AMC percentage cannot exceed 100.");
            }

            if (product != null
                && !string.IsNullOrWhiteSpace(vm.PricingModel)
                && await _repo.PricingModelExistsAsync(vm.ProductId, vm.PricingModel.Trim(), ignoreId))
            {
                ModelState.AddModelError(nameof(ProductRateFormViewModel.PricingModel), "This pricing model is already mapped for the selected product.");
            }
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Products = (await _productRepo.GetAllAsync()).ToList();
            ViewBag.PricingModels = (await _pricingModelRepo.GetAllActiveAsync()).ToList();
            ViewBag.AmcCalculationTypes = AmcCalculationTypes;
        }

        private static ProductRate CalculateAmc(decimal rate, string amcCalculationType, decimal amcInputValue)
        {
            var normalizedType = amcCalculationType.Trim();

            if (normalizedType == "Flat Amount")
            {
                var percentage = rate == 0 ? 0 : decimal.Round((amcInputValue / rate) * 100m, 4, System.MidpointRounding.AwayFromZero);
                return new ProductRate
                {
                    AmcCalculationType = normalizedType,
                    AmcAmount = decimal.Round(amcInputValue, 2, System.MidpointRounding.AwayFromZero),
                    AmcPercentage = percentage
                };
            }

            var amount = decimal.Round((rate * amcInputValue) / 100m, 2, System.MidpointRounding.AwayFromZero);
            return new ProductRate
            {
                AmcCalculationType = normalizedType,
                AmcPercentage = decimal.Round(amcInputValue, 4, System.MidpointRounding.AwayFromZero),
                AmcAmount = amount
            };
        }
    }
}
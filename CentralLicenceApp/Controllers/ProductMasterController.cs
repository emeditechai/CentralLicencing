using System.Linq;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductMasterController : Controller
    {
        private static readonly string[] ProductTypes = { "Healthcare", "Hospitality" };

        private readonly IProductMasterRepository _repo;
        private readonly IProductRateRepository _productRateRepo;

        public ProductMasterController(IProductMasterRepository repo, IProductRateRepository productRateRepo)
        {
            _repo = repo;
            _productRateRepo = productRateRepo;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _repo.GetAllAsync();
            return View(products.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return NotFound();

            var rates = (await _productRateRepo.GetAllAsync(id)).ToList();
            return View(new ProductMasterDetailsViewModel
            {
                Product = product,
                ProductRates = rates
            });
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return View(new ProductMasterFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductMasterFormViewModel vm)
        {
            PopulateDropdowns();
            await ValidateProductAsync(vm.ProductCode, vm.ProductType, null);
            if (!ModelState.IsValid) return View(vm);

            var product = new ProductMaster
            {
                ProductCode = vm.ProductCode.Trim(),
                ProductName = vm.ProductName.Trim(),
                ProductType = vm.ProductType.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(product);
            TempData["Success"] = $"Product <strong>{product.ProductName}</strong> created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return NotFound();

            PopulateDropdowns();
            return View(new ProductMasterFormViewModel
            {
                Id = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                ProductType = product.ProductType,
                IsActive = product.IsActive
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductMasterFormViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            PopulateDropdowns();
            await ValidateProductAsync(vm.ProductCode, vm.ProductType, vm.Id);
            if (!ModelState.IsValid) return View(vm);

            var product = new ProductMaster
            {
                Id = vm.Id,
                ProductCode = vm.ProductCode.Trim(),
                ProductName = vm.ProductName.Trim(),
                ProductType = vm.ProductType.Trim(),
                IsActive = vm.IsActive
            };

            await _repo.UpdateAsync(product);
            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteValidation = await _repo.ValidateDeleteAsync(id);
            if (!deleteValidation.CanDelete)
            {
                TempData["Error"] = deleteValidation.Reason;
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var deleted = await _repo.DeleteAsync(id);
                if (!deleted)
                {
                    TempData["Error"] = "The selected product was not found or could not be deleted.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (SqlException)
            {
                TempData["Error"] = "This product cannot be deleted because related records still reference it.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateProductAsync(string? productCode, string? productType, int? ignoreId)
        {
            if (!string.IsNullOrWhiteSpace(productCode)
                && await _repo.ProductCodeExistsAsync(productCode.Trim(), ignoreId))
            {
                ModelState.AddModelError(nameof(ProductMasterFormViewModel.ProductCode), "Product Code already exists.");
            }

            if (string.IsNullOrWhiteSpace(productType)
                || !ProductTypes.Contains(productType.Trim()))
            {
                ModelState.AddModelError(nameof(ProductMasterFormViewModel.ProductType), "Please select a valid Product Type.");
            }
        }

        private void PopulateDropdowns()
        {
            ViewBag.ProductTypes = ProductTypes;
        }
    }
}
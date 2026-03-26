using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CompanySettingsController : Controller
    {
        private readonly ICompanySettingsRepository _repo;
        private readonly IWebHostEnvironment _environment;

        public CompanySettingsController(ICompanySettingsRepository repo, IWebHostEnvironment environment)
        {
            _repo = repo;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var companies = await _repo.GetAllAsync();
            return View(companies.ToList());
        }

        public async Task<IActionResult> Create()
        {
            return View(new CompanySettingsFormViewModel
            {
                CompanyTypes = (await _repo.GetCompanyTypesAsync()).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanySettingsFormViewModel vm)
        {
            vm.CompanyTypes = (await _repo.GetCompanyTypesAsync()).ToList();

            if (!await _repo.CheckCompanyCodeUniqueAsync(vm.CompanyCode.Trim()))
                ModelState.AddModelError(nameof(vm.CompanyCode), "Company Code already exists.");

            if (!ModelState.IsValid) return View(vm);

            var company = new CompanySetting
            {
                CompanyCode = vm.CompanyCode.Trim(),
                CompanyTypeId = vm.CompanyTypeId,
                CompanyName = vm.CompanyName.Trim(),
                Country = vm.Country?.Trim(),
                State = vm.State?.Trim(),
                District = vm.District?.Trim(),
                City = vm.City?.Trim(),
                Address = vm.Address?.Trim(),
                Website = vm.Website?.Trim(),
                EmailId = vm.EmailId?.Trim(),
                ContactNo = vm.ContactNo?.Trim(),
                Pincode = vm.Pincode?.Trim(),
                GSTCode = vm.GSTCode?.Trim(),
                PANCard = vm.PANCard?.Trim(),
                IsParentCompany = vm.IsParentCompany,
                CompanyLogoPath = await SaveCompanyLogoAsync(vm.CompanyLogo),
                IsActive = vm.IsActive
            };

            await _repo.CreateAsync(company);
            TempData["Success"] = $"Company <strong>{company.CompanyName}</strong> created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await _repo.GetByIdAsync(id);
            if (company == null) return NotFound();

            return View(new CompanySettingsFormViewModel
            {
                Id = company.Id,
                CompanyCode = company.CompanyCode,
                CompanyTypeId = company.CompanyTypeId,
                CompanyName = company.CompanyName,
                Country = company.Country,
                State = company.State,
                District = company.District,
                City = company.City,
                Address = company.Address,
                Website = company.Website,
                EmailId = company.EmailId,
                ContactNo = company.ContactNo,
                Pincode = company.Pincode,
                GSTCode = company.GSTCode,
                PANCard = company.PANCard,
                IsParentCompany = company.IsParentCompany,
                ExistingLogoPath = company.CompanyLogoPath,
                IsActive = company.IsActive,
                CompanyTypes = (await _repo.GetCompanyTypesAsync()).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanySettingsFormViewModel vm)
        {
            vm.CompanyTypes = (await _repo.GetCompanyTypesAsync()).ToList();

            if (id != vm.Id) return BadRequest();

            if (!await _repo.CheckCompanyCodeUniqueAsync(vm.CompanyCode.Trim(), id))
                ModelState.AddModelError(nameof(vm.CompanyCode), "Company Code already exists.");

            if (!ModelState.IsValid) return View(vm);

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.CompanyCode = vm.CompanyCode.Trim();
            existing.CompanyTypeId = vm.CompanyTypeId;
            existing.CompanyName = vm.CompanyName.Trim();
            existing.Country = vm.Country?.Trim();
            existing.State = vm.State?.Trim();
            existing.District = vm.District?.Trim();
            existing.City = vm.City?.Trim();
            existing.Address = vm.Address?.Trim();
            existing.Website = vm.Website?.Trim();
            existing.EmailId = vm.EmailId?.Trim();
            existing.ContactNo = vm.ContactNo?.Trim();
            existing.Pincode = vm.Pincode?.Trim();
            existing.GSTCode = vm.GSTCode?.Trim();
            existing.PANCard = vm.PANCard?.Trim();
            existing.IsParentCompany = vm.IsParentCompany;
            existing.CompanyLogoPath = await SaveCompanyLogoAsync(vm.CompanyLogo, existing.CompanyLogoPath);
            existing.IsActive = vm.IsActive;

            await _repo.UpdateAsync(existing);
            TempData["Success"] = "Company settings updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await _repo.GetByIdAsync(id);
            if (company == null) return NotFound();

            DeleteCompanyLogo(company.CompanyLogoPath);
            await _repo.DeleteAsync(id);
            TempData["Success"] = "Company settings deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveCompanyLogoAsync(Microsoft.AspNetCore.Http.IFormFile? file, string? existingPath = null)
        {
            if (file == null || file.Length == 0)
                return existingPath;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return existingPath;

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "company-logos");
            Directory.CreateDirectory(uploadsRoot);

            DeleteCompanyLogo(existingPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream);

            return $"/uploads/company-logos/{fileName}";
        }

        private void DeleteCompanyLogo(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            var existingFullPath = Path.Combine(_environment.WebRootPath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(existingFullPath))
                System.IO.File.Delete(existingFullPath);
        }
    }
}
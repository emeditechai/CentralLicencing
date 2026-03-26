using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly ILocationRepository _locationRepo;
        private readonly IEmployeeDepartmentRepository _departmentRepo;
        private readonly IEmployeeDesignationRepository _designationRepo;
        private readonly IEmployeeTypeRepository _employeeTypeRepo;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepo, IRoleRepository roleRepo, ILocationRepository locationRepo, IEmployeeDepartmentRepository departmentRepo, IEmployeeDesignationRepository designationRepo, IEmployeeTypeRepository employeeTypeRepo, IEmailService emailService, IWebHostEnvironment environment, ILogger<UserController> logger)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _locationRepo = locationRepo;
            _departmentRepo = departmentRepo;
            _designationRepo = designationRepo;
            _employeeTypeRepo = employeeTypeRepo;
            _emailService = emailService;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepo.GetAllAsync();
            return View(users.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        public async Task<IActionResult> Create()
        {
            var vm = new UserFormViewModel
            {
                Roles     = (await _roleRepo.GetAllAsync()).ToList(),
                Locations = (await _locationRepo.GetAllActiveAsync()).ToList(),
                Managers  = (await _userRepo.GetEmployeesAsync()).ToList(),
                Departments = (await _departmentRepo.GetAllActiveAsync()).ToList(),
                Designations = (await _designationRepo.GetAllActiveAsync()).ToList(),
                EmployeeTypes = (await _employeeTypeRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            vm.Roles     = (await _roleRepo.GetAllAsync()).ToList();
            vm.Locations = (await _locationRepo.GetAllActiveAsync()).ToList();
            vm.Managers  = (await _userRepo.GetEmployeesAsync()).ToList();
            vm.Departments = (await _departmentRepo.GetAllActiveAsync()).ToList();
            vm.Designations = (await _designationRepo.GetAllActiveAsync()).ToList();
            vm.EmployeeTypes = (await _employeeTypeRepo.GetAllActiveAsync()).ToList();

            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Password is required for new users.");

            if (vm.IsEmployee)
            {
                if (string.IsNullOrWhiteSpace(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code is required when Is Employee is checked.");
                else if (!await _userRepo.CheckEmployeeCodeUniqueAsync(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code already exists.");

                if (!vm.IsCoreMember && vm.ManagerId == null)
                    ModelState.AddModelError("ManagerId", "Manager is required unless the user is a Core Member.");

                if (vm.DepartmentId == null)
                    ModelState.AddModelError("DepartmentId", "Department is required when Is Employee is checked.");

                if (vm.DesignationId == null)
                    ModelState.AddModelError("DesignationId", "Designation is required when Is Employee is checked.");

                if (vm.EmployeeTypeId == null)
                    ModelState.AddModelError("EmployeeTypeId", "Employee Type is required when Is Employee is checked.");
            }

            if (!ModelState.IsValid) return View(vm);

            var user = new UserMaster
            {
                Username      = vm.Username.Trim(),
                Email         = vm.Email.Trim(),
                FullName      = vm.FullName?.Trim(),
                PhoneNumber   = vm.PhoneNumber?.Trim(),
                RoleId        = vm.RoleId,
                LocationId    = vm.LocationId,
                DepartmentId  = vm.IsEmployee ? vm.DepartmentId : null,
                DesignationId = vm.IsEmployee ? vm.DesignationId : null,
                EmployeeTypeId = vm.IsEmployee ? vm.EmployeeTypeId : null,
                IsEmployee    = vm.IsEmployee,
                EmployeeCode  = vm.IsEmployee ? vm.EmployeeCode?.Trim() : null,
                IsCoreMember  = vm.IsEmployee && vm.IsCoreMember,
                ManagerId     = vm.IsEmployee ? vm.ManagerId : null,
                ProfileImagePath = await SaveProfileImageAsync(vm.ProfileImage),
                IsActive      = vm.IsActive,
                PasswordHash  = BCrypt.Net.BCrypt.HashPassword(vm.Password!)
            };

            var newUserId = await _userRepo.CreateAsync(user);
            user.Id = newUserId;

            await SendOnboardingEmailAsync(newUserId, vm.Password);

            TempData["Success"] = $"User <strong>{user.Username}</strong> created successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null) return NotFound();

            var vm = new UserFormViewModel
            {
                Id           = user.Id,
                Username     = user.Username,
                Email        = user.Email,
                FullName     = user.FullName,
                PhoneNumber  = user.PhoneNumber,
                RoleId       = user.RoleId,
                LocationId   = user.LocationId,
                DepartmentId = user.DepartmentId,
                DesignationId = user.DesignationId,
                EmployeeTypeId = user.EmployeeTypeId,
                IsEmployee   = user.IsEmployee,
                EmployeeCode = user.EmployeeCode,
                IsCoreMember = user.IsCoreMember,
                ManagerId    = user.ManagerId,
                ExistingProfileImagePath = user.ProfileImagePath,
                IsActive     = user.IsActive,
                Roles        = (await _roleRepo.GetAllAsync()).ToList(),
                Locations    = (await _locationRepo.GetAllActiveAsync()).ToList(),
                Managers     = (await _userRepo.GetEmployeesAsync()).ToList(),
                Departments = (await _departmentRepo.GetAllActiveAsync()).ToList(),
                Designations = (await _designationRepo.GetAllActiveAsync()).ToList(),
                EmployeeTypes = (await _employeeTypeRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
        {
            vm.Roles     = (await _roleRepo.GetAllAsync()).ToList();
            vm.Locations = (await _locationRepo.GetAllActiveAsync()).ToList();
            vm.Managers  = (await _userRepo.GetEmployeesAsync()).ToList();
            vm.Departments = (await _departmentRepo.GetAllActiveAsync()).ToList();
            vm.Designations = (await _designationRepo.GetAllActiveAsync()).ToList();
            vm.EmployeeTypes = (await _employeeTypeRepo.GetAllActiveAsync()).ToList();

            ModelState.Remove("Username");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (vm.IsEmployee)
            {
                if (string.IsNullOrWhiteSpace(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code is required when Is Employee is checked.");
                else if (!await _userRepo.CheckEmployeeCodeUniqueAsync(vm.EmployeeCode, id))
                    ModelState.AddModelError("EmployeeCode", "Employee Code already exists.");

                if (!vm.IsCoreMember && vm.ManagerId == null)
                    ModelState.AddModelError("ManagerId", "Manager is required unless the user is a Core Member.");

                if (vm.DepartmentId == null)
                    ModelState.AddModelError("DepartmentId", "Department is required when Is Employee is checked.");

                if (vm.DesignationId == null)
                    ModelState.AddModelError("DesignationId", "Designation is required when Is Employee is checked.");

                if (vm.EmployeeTypeId == null)
                    ModelState.AddModelError("EmployeeTypeId", "Employee Type is required when Is Employee is checked.");
            }

            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Email        = vm.Email.Trim();
            existing.FullName     = vm.FullName?.Trim();
            existing.PhoneNumber  = vm.PhoneNumber?.Trim();
            existing.RoleId       = vm.RoleId;
            existing.LocationId   = vm.LocationId;
            existing.DepartmentId = vm.IsEmployee ? vm.DepartmentId : null;
            existing.DesignationId = vm.IsEmployee ? vm.DesignationId : null;
            existing.EmployeeTypeId = vm.IsEmployee ? vm.EmployeeTypeId : null;
            existing.IsEmployee   = vm.IsEmployee;
            existing.EmployeeCode = vm.IsEmployee ? vm.EmployeeCode?.Trim() : null;
            existing.IsCoreMember = vm.IsEmployee && vm.IsCoreMember;
            existing.ManagerId    = vm.IsEmployee ? vm.ManagerId : null;
            if (vm.ProfileImage != null)
                existing.ProfileImagePath = await SaveProfileImageAsync(vm.ProfileImage, existing.ProfileImagePath);
            existing.IsActive     = vm.IsActive;

            await _userRepo.UpdateAsync(existing);

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var newHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!);
                await ((UserRepository)_userRepo).UpdatePasswordAsync(id, newHash);
            }

            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserIdClaim == id.ToString())
                await RefreshCurrentUserClaimsAsync(existing);

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            if (id == currentUserId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            await _userRepo.DeleteAsync(id);
            TempData["Success"] = "User deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckEmployeeCode(string code, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(code)) return Json(true);
            var isUnique = await _userRepo.CheckEmployeeCodeUniqueAsync(code, excludeId);
            return Json(isUnique);
        }

        private async Task<string?> SaveProfileImageAsync(Microsoft.AspNetCore.Http.IFormFile? file, string? existingPath = null)
        {
            if (file == null || file.Length == 0)
                return existingPath;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(extension))
                return existingPath;

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
            Directory.CreateDirectory(uploadsRoot);

            if (!string.IsNullOrWhiteSpace(existingPath))
            {
                var existingFullPath = Path.Combine(_environment.WebRootPath, existingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(existingFullPath))
                    System.IO.File.Delete(existingFullPath);
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);
            await using var stream = System.IO.File.Create(physicalPath);
            await file.CopyToAsync(stream);

            return $"/uploads/profile-pictures/{fileName}";
        }

        private async Task RefreshCurrentUserClaimsAsync(UserMaster user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new("FullName", user.FullName ?? user.Username),
                new(ClaimTypes.Role, user.RoleName ?? "Staff"),
                new("ProfileImagePath", user.ProfileImagePath ?? string.Empty),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private async Task SendOnboardingEmailAsync(int userId, string? temporaryPassword)
        {
            var createdUser = await _userRepo.GetByIdAsync(userId);
            if (createdUser == null || string.IsNullOrWhiteSpace(createdUser.Email))
                return;

            var request = HttpContext.Request;
            var loginUrl = $"{request.Scheme}://{request.Host}{Url.Action("Login", "Account")}";

            var placeholders = new Dictionary<string, string>
            {
                ["Username"] = createdUser.Username,
                ["Email"] = createdUser.Email,
                ["FullName"] = string.IsNullOrWhiteSpace(createdUser.FullName) ? createdUser.Username : createdUser.FullName,
                ["PhoneNumber"] = GetDisplayValue(createdUser.PhoneNumber),
                ["RoleName"] = GetDisplayValue(createdUser.RoleName),
                ["LocationName"] = GetDisplayValue(createdUser.LocationName),
                ["DepartmentName"] = GetDisplayValue(createdUser.DepartmentName),
                ["DesignationName"] = GetDisplayValue(createdUser.DesignationName),
                ["EmployeeCode"] = GetDisplayValue(createdUser.EmployeeCode),
                ["ManagerName"] = GetDisplayValue(createdUser.ManagerName),
                ["IsCoreMember"] = createdUser.IsEmployee ? (createdUser.IsCoreMember ? "Yes" : "No") : "No",
                ["Status"] = createdUser.IsActive ? "Active" : "Inactive",
                ["LoginUrl"] = loginUrl,
                ["TemporaryPassword"] = GetDisplayValue(temporaryPassword)
            };

            try
            {
                await _emailService.SendTemplatedAsync(
                    "USER_ONBOARDING",
                    createdUser.Email,
                    placeholders["FullName"],
                    placeholders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send onboarding email for user id {UserId}", userId);
            }
        }

        private static string GetDisplayValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "N/A" : value.Trim();
        }
    }
}


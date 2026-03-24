using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly ILocationRepository _locationRepo;

        public UserController(IUserRepository userRepo, IRoleRepository roleRepo, ILocationRepository locationRepo)
        {
            _userRepo     = userRepo;
            _roleRepo     = roleRepo;
            _locationRepo = locationRepo;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userRepo.GetAllAsync();
            return View(users.ToList());
        }

        public async Task<IActionResult> Create()
        {
            var vm = new UserFormViewModel
            {
                Roles     = (await _roleRepo.GetAllAsync()).ToList(),
                Locations = (await _locationRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            vm.Roles     = (await _roleRepo.GetAllAsync()).ToList();
            vm.Locations = (await _locationRepo.GetAllActiveAsync()).ToList();

            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Password is required for new users.");

            if (vm.IsEmployee)
            {
                if (string.IsNullOrWhiteSpace(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code is required when Is Employee is checked.");
                else if (!await _userRepo.CheckEmployeeCodeUniqueAsync(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code already exists.");
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
                IsEmployee    = vm.IsEmployee,
                EmployeeCode  = vm.IsEmployee ? vm.EmployeeCode?.Trim() : null,
                IsActive      = vm.IsActive,
                PasswordHash  = BCrypt.Net.BCrypt.HashPassword(vm.Password!)
            };

            await _userRepo.CreateAsync(user);
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
                IsEmployee   = user.IsEmployee,
                EmployeeCode = user.EmployeeCode,
                IsActive     = user.IsActive,
                Roles        = (await _roleRepo.GetAllAsync()).ToList(),
                Locations    = (await _locationRepo.GetAllActiveAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
        {
            vm.Roles     = (await _roleRepo.GetAllAsync()).ToList();
            vm.Locations = (await _locationRepo.GetAllActiveAsync()).ToList();

            ModelState.Remove("Username");
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (vm.IsEmployee)
            {
                if (string.IsNullOrWhiteSpace(vm.EmployeeCode))
                    ModelState.AddModelError("EmployeeCode", "Employee Code is required when Is Employee is checked.");
                else if (!await _userRepo.CheckEmployeeCodeUniqueAsync(vm.EmployeeCode, id))
                    ModelState.AddModelError("EmployeeCode", "Employee Code already exists.");
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
            existing.IsEmployee   = vm.IsEmployee;
            existing.EmployeeCode = vm.IsEmployee ? vm.EmployeeCode?.Trim() : null;
            existing.IsActive     = vm.IsActive;

            await _userRepo.UpdateAsync(existing);

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var newHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!);
                await ((UserRepository)_userRepo).UpdatePasswordAsync(id, newHash);
            }

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
    }
}


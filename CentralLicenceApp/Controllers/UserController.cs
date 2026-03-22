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

        public UserController(IUserRepository userRepo, IRoleRepository roleRepo)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
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
                Roles = (await _roleRepo.GetAllAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel vm)
        {
            vm.Roles = (await _roleRepo.GetAllAsync()).ToList();

            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "Password is required for new users.");

            if (!ModelState.IsValid) return View(vm);

            var user = new UserMaster
            {
                Username     = vm.Username.Trim(),
                Email        = vm.Email.Trim(),
                FullName     = vm.FullName?.Trim(),
                RoleId       = vm.RoleId,
                IsActive     = vm.IsActive,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!)
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
                Id       = user.Id,
                Username = user.Username,
                Email    = user.Email,
                FullName = user.FullName,
                RoleId   = user.RoleId,
                IsActive = user.IsActive,
                Roles    = (await _roleRepo.GetAllAsync()).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserFormViewModel vm)
        {
            vm.Roles = (await _roleRepo.GetAllAsync()).ToList();
            // password is optional on edit
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid) return View(vm);

            var existing = await _userRepo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Email    = vm.Email.Trim();
            existing.FullName = vm.FullName?.Trim();
            existing.RoleId   = vm.RoleId;
            existing.IsActive = vm.IsActive;

            await _userRepo.UpdateAsync(existing);

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var newHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!);
                // Update password via repository method
                await ((UserRepository)_userRepo).UpdatePasswordAsync(id, newHash);
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // Prevent deleting your own account
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
    }
}

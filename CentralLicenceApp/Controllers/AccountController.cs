using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using CentralLicenceApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IDataProtector _roleSelectionProtector;
        private readonly IDataProtector _passwordResetProtector;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IUserRepository userRepo, IDataProtectionProvider dataProtectionProvider, IEmailService emailService, IWebHostEnvironment environment)
        {
            _userRepo = userRepo;
            _roleSelectionProtector = dataProtectionProvider.CreateProtector("AccountController.PendingRoleSelection.v1");
            _passwordResetProtector = dataProtectionProvider.CreateProtector("AccountController.PasswordReset.v1");
            _emailService = emailService;
            _environment = environment;
        }

        private static readonly HashSet<string> CrmOnlyRoles = new(StringComparer.OrdinalIgnoreCase)
            { "ClientTicket", "Ticket Admin", "Ticket Agent" };

        private bool IsCrmOnlyRole() => CrmOnlyRoles.Contains(User.FindFirstValue(ClaimTypes.Role) ?? "");

        private IActionResult RedirectToDefaultHome()
            => IsCrmOnlyRole()
                ? RedirectToAction("MyTickets", "HelpDeskTicket")
                : RedirectToAction("Index", "Dashboard");

        private IActionResult RedirectToDefaultHome(string roleName)
            => CrmOnlyRoles.Contains(roleName)
                ? RedirectToAction("MyTickets", "HelpDeskTicket")
                : RedirectToAction("Index", "Dashboard");

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToDefaultHome();

            ApplyNoCacheHeaders();
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ApplyNoCacheHeaders();
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                model.Password = string.Empty;
                ModelState.Remove(nameof(LoginViewModel.Password));
                return View(model);
            }

            var user = await _userRepo.GetByUsernameAsync(model.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                model.Password = string.Empty;
                ModelState.Remove(nameof(LoginViewModel.Password));
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var assignedRoles = user.Roles
                .Where(role => role.IsActive)
                .OrderBy(role => role.RoleName)
                .ToList();

            if (!assignedRoles.Any())
            {
                model.Password = string.Empty;
                ModelState.Remove(nameof(LoginViewModel.Password));
                ModelState.AddModelError(string.Empty, "No active role is assigned to this account.");
                return View(model);
            }

            if (assignedRoles.Count > 1)
            {
                return View(BuildRoleSelectionModel(user, model.RememberMe, returnUrl));
            }

            await SignInWithRoleAsync(user, assignedRoles[0].Id, model.RememberMe);

            var loginRoleName = assignedRoles[0].RoleName;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToDefaultHome(loginRoleName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> SelectRole(int selectedRoleId, string pendingSelectionToken)
        {
            ApplyNoCacheHeaders();

            PendingRoleSelectionPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<PendingRoleSelectionPayload>(
                    _roleSelectionProtector.Unprotect(pendingSelectionToken));
            }
            catch
            {
                TempData["Error"] = "Your login session expired. Please sign in again.";
                return RedirectToAction(nameof(Login));
            }

            if (payload == null || payload.IssuedAtUtc < DateTimeOffset.UtcNow.AddMinutes(-10))
            {
                TempData["Error"] = "Your login session expired. Please sign in again.";
                return RedirectToAction(nameof(Login), new { returnUrl = payload?.ReturnUrl });
            }

            var user = await _userRepo.GetByIdAsync(payload.UserId);
            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "This account is no longer available. Please sign in again.";
                return RedirectToAction(nameof(Login), new { returnUrl = payload.ReturnUrl });
            }

            var selectedRole = user.Roles.FirstOrDefault(role => role.IsActive && role.Id == selectedRoleId);
            if (selectedRole == null)
            {
                ModelState.AddModelError(string.Empty, "Select a valid role to continue.");
                ViewData["ReturnUrl"] = payload.ReturnUrl;
                return View("Login", BuildRoleSelectionModel(user, payload.RememberMe, payload.ReturnUrl));
            }

            await SignInWithRoleAsync(user, selectedRole.Id, payload.RememberMe);

            if (!string.IsNullOrEmpty(payload.ReturnUrl) && Url.IsLocalUrl(payload.ReturnUrl))
                return Redirect(payload.ReturnUrl);

            return RedirectToDefaultHome(selectedRole.RoleName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SwitchRole(int selectedRoleId, string? returnUrl)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            var selectedRole = user.Roles.FirstOrDefault(role => role.IsActive && role.Id == selectedRoleId);
            if (selectedRole == null)
            {
                TempData["Error"] = "The selected role is not available for this account.";
                return RedirectToLocal(returnUrl);
            }

            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await SignInWithRoleAsync(user, selectedRole.Id, authResult.Properties);
            TempData["Success"] = $"Active role changed to <strong>{selectedRole.RoleName}</strong>.";

            return RedirectToLocal(returnUrl);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CurrentPassword = string.Empty;
                model.NewPassword = string.Empty;
                model.ConfirmNewPassword = string.Empty;
                return View(model);
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Error"] = "Your session expired. Please sign in again.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["Error"] = "Your account is no longer available. Please sign in again.";
                return RedirectToAction(nameof(Login));
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Current password is incorrect.");
                model.CurrentPassword = string.Empty;
                model.NewPassword = string.Empty;
                model.ConfirmNewPassword = string.Empty;
                return View(model);
            }

            if (BCrypt.Net.BCrypt.Verify(model.NewPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), "New password must be different from the current password.");
                model.CurrentPassword = string.Empty;
                model.NewPassword = string.Empty;
                model.ConfirmNewPassword = string.Empty;
                return View(model);
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepo.UpdatePasswordAsync(user.Id, newHash);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Password changed successfully. Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        private void ApplyNoCacheHeaders()
        {
            Response.Headers.CacheControl = "no-store, no-cache, max-age=0, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
        }

        // ── Forgot Password ──────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepo.GetByEmailAsync(model.Email.Trim());
            if (user != null)
            {
                var payload = new PasswordResetPayload
                {
                    UserId = user.Id,
                    IssuedAtUtc = DateTimeOffset.UtcNow
                };
                var token = _passwordResetProtector.Protect(JsonSerializer.Serialize(payload));
                var resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

                await _emailService.SendTemplatedAsync("PASSWORD_RESET", user.Email, user.FullName ?? user.Username,
                    new Dictionary<string, string>
                    {
                        ["FullName"] = user.FullName ?? user.Username,
                        ["ResetUrl"] = resetUrl!
                    });
            }

            // Always show success to prevent email enumeration
            TempData["ForgotPasswordSent"] = true;
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            PasswordResetPayload? payload;
            try
            {
                payload = JsonSerializer.Deserialize<PasswordResetPayload>(
                    _passwordResetProtector.Unprotect(model.Token));
            }
            catch
            {
                TempData["Error"] = "The password reset link is invalid or has been tampered with. Please request a new one.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            if (payload == null || payload.IssuedAtUtc < DateTimeOffset.UtcNow.AddMinutes(-30))
            {
                TempData["Error"] = "The password reset link has expired. Please request a new one.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var user = await _userRepo.GetByIdAsync(payload.UserId);
            if (user == null || !user.IsActive)
            {
                TempData["Error"] = "This account is no longer available.";
                return RedirectToAction(nameof(Login));
            }

            var newHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepo.UpdatePasswordAsync(user.Id, newHash);

            TempData["Success"] = "Your password has been reset successfully. Please sign in with your new password.";
            return RedirectToAction(nameof(Login));
        }

        private LoginViewModel BuildRoleSelectionModel(UserMaster user, bool rememberMe, string? returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var payload = new PendingRoleSelectionPayload
            {
                UserId = user.Id,
                RememberMe = rememberMe,
                ReturnUrl = returnUrl,
                IssuedAtUtc = DateTimeOffset.UtcNow
            };

            return new LoginViewModel
            {
                Username = user.Username,
                RememberMe = rememberMe,
                ShowRoleSelectionPopup = true,
                PendingSelectionToken = _roleSelectionProtector.Protect(JsonSerializer.Serialize(payload)),
                AvailableRoles = user.Roles
                    .Where(role => role.IsActive)
                    .OrderBy(role => role.RoleName)
                    .ToList()
            };
        }

        // ── Change Profile Picture (self-service from topbar) ──

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeProfilePicture(IFormFile? profileImage)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            if (profileImage == null || profileImage.Length == 0)
                return Json(new { success = false, message = "Please select an image file." });

            if (profileImage.Length > 2 * 1024 * 1024)
                return Json(new { success = false, message = "Image must be under 2 MB." });

            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            var allowed = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowed.Contains(extension))
                return Json(new { success = false, message = "Only JPG, PNG, GIF, and WEBP images are allowed." });

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // Delete old profile picture
            if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Save new image
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsRoot, fileName);
            await using (var stream = System.IO.File.Create(physicalPath))
            {
                await profileImage.CopyToAsync(stream);
            }

            user.ProfileImagePath = $"/uploads/profile-pictures/{fileName}";
            await _userRepo.UpdateAsync(user);

            // Refresh claims so topbar updates immediately
            await RefreshClaimsAsync(user);

            return Json(new { success = true, message = "Profile picture updated!", imageUrl = user.ProfileImagePath });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePicture()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Json(new { success = false, message = "Session expired." });

            var user = await _userRepo.GetByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ProfileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            user.ProfileImagePath = null;
            await _userRepo.UpdateAsync(user);
            await RefreshClaimsAsync(user);

            return Json(new { success = true, message = "Profile picture removed." });
        }

        private async Task RefreshClaimsAsync(UserMaster user)
        {
            var activeRoleId = User.FindFirst("ActiveRoleId")?.Value;
            var selectedRole = user.Roles.FirstOrDefault(r => r.Id.ToString() == activeRoleId)
                ?? user.Roles.FirstOrDefault(r => r.Id == user.RoleId)
                ?? user.Roles.FirstOrDefault();
            var roleName = selectedRole?.RoleName ?? user.RoleName ?? "Staff";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new("FullName", user.FullName ?? user.Username),
                new(ClaimTypes.Role, roleName),
                new("ActiveRoleId", (selectedRole?.Id ?? user.RoleId).ToString()),
                new("ProfileImagePath", user.ProfileImagePath ?? string.Empty),
            };

            if (string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase))
                claims.Add(new Claim("IsSuperAdmin", "true"));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private async Task SignInWithRoleAsync(UserMaster user, int roleId, bool rememberMe)
        {
            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await SignInWithRoleAsync(user, roleId, authProps);
        }

        private async Task SignInWithRoleAsync(UserMaster user, int roleId, AuthenticationProperties? authProps)
        {
            var selectedRole = user.Roles.FirstOrDefault(role => role.Id == roleId) ?? user.Roles.FirstOrDefault();
            var roleName = selectedRole?.RoleName ?? user.RoleName ?? "Staff";

            await _userRepo.UpdateLastLoginAsync(user.Id);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new("FullName", user.FullName ?? user.Username),
                new(ClaimTypes.Role, roleName),
                new("ActiveRoleId", (selectedRole?.Id ?? user.RoleId).ToString()),
                new("ProfileImagePath", user.ProfileImagePath ?? string.Empty),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps ?? new AuthenticationProperties());
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToDefaultHome();
        }

        private sealed class PendingRoleSelectionPayload
        {
            public int UserId { get; set; }
            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
            public DateTimeOffset IssuedAtUtc { get; set; }
        }

        private sealed class PasswordResetPayload
        {
            public int UserId { get; set; }
            public DateTimeOffset IssuedAtUtc { get; set; }
        }
    }
}

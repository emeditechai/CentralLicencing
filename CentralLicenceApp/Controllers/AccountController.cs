using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IDataProtector _roleSelectionProtector;

        public AccountController(IUserRepository userRepo, IDataProtectionProvider dataProtectionProvider)
        {
            _userRepo = userRepo;
            _roleSelectionProtector = dataProtectionProvider.CreateProtector("AccountController.PendingRoleSelection.v1");
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
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

            return RedirectToAction("Index", "Dashboard");
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

        private void ApplyNoCacheHeaders()
        {
            Response.Headers.CacheControl = "no-store, no-cache, max-age=0, must-revalidate";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
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

            return RedirectToAction("Index", "Dashboard");
        }

        private sealed class PendingRoleSelectionPayload
        {
            public int UserId { get; set; }
            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
            public DateTimeOffset IssuedAtUtc { get; set; }
        }
    }
}

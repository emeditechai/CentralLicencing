using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CentralLicenceApp.Models;
using CentralLicenceApp.Models.ViewModels;
using CentralLicenceApp.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    [Route("push-notifications")]
    public class PushNotificationController : Controller
    {
        private readonly IUserPushSubscriptionRepository _subscriptionRepository;
        private readonly PushNotificationSettings _pushSettings;

        public PushNotificationController(
            IUserPushSubscriptionRepository subscriptionRepository,
            IOptions<PushNotificationSettings> pushSettings)
        {
            _subscriptionRepository = subscriptionRepository;
            _pushSettings = pushSettings.Value;
        }

        [HttpGet("public-key")]
        public IActionResult PublicKey()
        {
            return Ok(new
            {
                enabled = _pushSettings.IsConfigured,
                publicKey = _pushSettings.PublicKey
            });
        }

        [HttpPost("subscribe")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionRequest request)
        {
            if (!_pushSettings.IsConfigured)
            {
                return BadRequest(new { message = "Web push is not configured." });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var subscription = new UserPushSubscription
            {
                UserId = userId.Value,
                Endpoint = request.Endpoint.Trim(),
                P256dh = request.P256dh.Trim(),
                Auth = request.Auth.Trim(),
                UserAgent = Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };

            await _subscriptionRepository.UpsertAsync(subscription);
            return Ok(new { success = true });
        }

        [HttpPost("unsubscribe")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsubscribe([FromBody] PushSubscriptionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Endpoint))
            {
                return BadRequest(new { message = "Endpoint is required." });
            }

            await _subscriptionRepository.DeactivateAsync(userId.Value, request.Endpoint.Trim());
            return Ok(new { success = true });
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}
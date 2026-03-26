using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace CentralLicenceApp.Services
{
    public sealed class AdminAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden && IsSuperAdmin(context.User))
            {
                await next(context);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static bool IsSuperAdmin(ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated == true
                && (string.Equals(user.Identity?.Name, "admin", StringComparison.OrdinalIgnoreCase)
                    || user.HasClaim("IsSuperAdmin", "true"));
        }
    }
}
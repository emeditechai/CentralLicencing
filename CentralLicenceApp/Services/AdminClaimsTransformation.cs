using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace CentralLicenceApp.Services
{
    public sealed class AdminClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult(principal);
            }

            var username = principal.Identity?.Name;
            if (!string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(principal);
            }

            if (principal.Identity is not ClaimsIdentity identity)
            {
                return Task.FromResult(principal);
            }

            AddRoleClaimIfMissing(identity, "Administrator");
            AddRoleClaimIfMissing(identity, "Finance");
            identity.AddClaimIfMissing(new Claim("IsSuperAdmin", "true"));

            return Task.FromResult(principal);
        }

        private static void AddRoleClaimIfMissing(ClaimsIdentity identity, string role)
        {
            if (!identity.HasClaim(ClaimTypes.Role, role))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }
    }

    internal static class ClaimsIdentityExtensions
    {
        public static void AddClaimIfMissing(this ClaimsIdentity identity, Claim claim)
        {
            if (!identity.HasClaim(claim.Type, claim.Value))
            {
                identity.AddClaim(claim);
            }
        }
    }
}
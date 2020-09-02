
namespace Security {
    using System;
    using System.Security.Claims;

    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetTenantId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("tenant_id")?.Value;
        }

        public static bool IsTenant(this ClaimsPrincipal principal, string id)
        {
            var tenantId = GetTenantId(principal);
            return string.Equals(tenantId, id, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsUser(this ClaimsPrincipal principal, string id)
        {
            var userId = GetUserId(principal);
            return string.Equals(userId, id, StringComparison.OrdinalIgnoreCase);
        }
    }
}
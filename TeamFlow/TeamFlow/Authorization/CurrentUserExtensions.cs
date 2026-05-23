using System.Security.Claims;

namespace TeamFlow.Authorization
{
    public static class CurrentUserExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }

        public static string? GetRole(this ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.Role);
    }
}

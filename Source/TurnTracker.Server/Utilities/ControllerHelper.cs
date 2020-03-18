using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using TurnTracker.Domain.Authorization;

namespace TurnTracker.Server.Utilities
{
    public static class ControllerHelper
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public static int? TryGetId(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (int?)null;
        }

        public static long GetLoginId(this ClaimsPrincipal user)
        {
            return long.Parse(user.FindFirstValue(nameof(ClaimType.LoginId)));
        }

        public static string GetRefreshKey(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(nameof(ClaimType.RefreshKey));
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Name);
        }

        public static string GetDisplayName(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.GivenName);
        }

        public static string GetDeviceName(this HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].ToString();

            // find the first string inside parenthesis
            var match = Regex.Match(userAgent, @"^[^)]*\(([^)]+)\)");

            return match.Success ? match.Groups[0].Value : userAgent;
        }
    }
}
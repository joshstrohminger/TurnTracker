using System.Security.Claims;
using TurnTracker.Domain.Authorization;

namespace TurnTracker.Server.Utilities
{
    public static class ControllerHelper
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        public static long GetLoginId(this ClaimsPrincipal user)
        {
            return long.Parse(user.FindFirstValue(nameof(ClaimType.LoginId)));
        }

        public static string GetRefreshKey(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(nameof(ClaimType.RefreshKey));
        }
    }
}
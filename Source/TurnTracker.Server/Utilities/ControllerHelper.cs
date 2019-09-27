using System.Security.Claims;
using TurnTracker.Data.Entities;

namespace TurnTracker.Server.Utilities
{
    public static class ControllerHelper
    {
        public static int GetId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }
}
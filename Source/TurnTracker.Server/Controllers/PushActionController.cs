using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize(Policy = nameof(PolicyType.CanActOnNotification))]
    [Route("api/notification/push/act")]
    public class PushActionController : Controller
    {
        #region Fields

        private readonly IPushNotificationActionService _pushNotificationActionService;

        #endregion Fields

        #region Ctor

        public PushActionController(IPushNotificationActionService pushNotificationActionService)
        {
            _pushNotificationActionService = pushNotificationActionService;
        }

        #endregion Ctor

        #region Endpoints

        [HttpPost]
        public async Task<IActionResult> Act()
        {
            var action = User.FindFirstValue(nameof(ClaimType.NotificationAction));
            var participantId = User.FindFirstValue(nameof(ClaimType.ParticipantId));

            var result = await _pushNotificationActionService.ActAsync(User.GetId(), int.Parse(participantId), action);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion Endpoints
    }
}
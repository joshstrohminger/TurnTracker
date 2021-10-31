using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize(Policy = nameof(PolicyType.CanActOnNotification))]
    [Route("api/notification/push/act")]
    public class PushActionController : Controller
    {
        #region Fields

        private readonly IPushNotificationActionService _pushNotificationActionService;
        private readonly ILogger<PushActionController> _logger;

        #endregion Fields

        #region Ctor

        public PushActionController(IPushNotificationActionService pushNotificationActionService, ILogger<PushActionController> logger)
        {
            _pushNotificationActionService = pushNotificationActionService;
            _logger = logger;
        }

        #endregion Ctor

        #region Endpoints

        [HttpPost]
        public async Task<IActionResult> Act([FromBody] PushTime actionTime)
        {
            var action = User.FindFirstValue(nameof(ClaimType.NotificationAction));
            var participantId = User.FindFirstValue(nameof(ClaimType.ParticipantId));
            var when = actionTime.Parse();

            _logger.LogInformation($"Action: {action}, When: {actionTime.When}, Parsed: {when}");

            var result = await _pushNotificationActionService.ActAsync(User.GetId(), int.Parse(participantId), action, when);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion Endpoints
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        #region Fields

        private readonly INotificationService _notificationService;

        #endregion

        #region ctor

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        #endregion

        #region Endpoints

        [HttpPost]
        public IActionResult Save([FromBody] NotificationSetting setting)
        {
            var result = _notificationService.UpdateNotificationSetting(setting.ParticipantId, setting.Type,
                setting.Sms, setting.Email, setting.Push);
            if (result.IsSuccess)
            {
                return Ok();
            }

            return BadRequest();
        }

        #endregion
    }
}
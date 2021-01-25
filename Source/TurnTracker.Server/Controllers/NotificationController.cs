using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class NotificationController : Controller
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        #endregion

        #region ctor

        public NotificationController(INotificationService notificationService, IResourceAuthorizationService resourceAuthorizationService)
        {
            _notificationService = notificationService;
            _resourceAuthorizationService = resourceAuthorizationService;
        }

        #endregion

        #region Endpoints

        [HttpPost]
        public IActionResult Save([FromBody] NotificationInfo info)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_resourceAuthorizationService.CanModifyParticipant(info.ParticipantId, User.GetId())) return Forbid();

            var result = _notificationService.UpdateNotificationSetting(info);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        [HttpPut("{participantId}/[action]/{dismissTimeOfDay}")]
        public async Task<IActionResult> DismissTimeOfDay(int participantId, string dismissTimeOfDay)
        {
            if (!_resourceAuthorizationService.CanModifyParticipant(participantId, User.GetId())) return Forbid();

            if (!TimeSpan.TryParse(dismissTimeOfDay, out var time)) return BadRequest();

            var result = await _notificationService.UpdateDismissTimeOfDayAsync(participantId, time);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion
    }
}
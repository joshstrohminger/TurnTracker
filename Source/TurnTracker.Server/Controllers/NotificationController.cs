﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;
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
        public IActionResult Save([FromBody] NotificationSetting setting)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_resourceAuthorizationService.CanModifyParticipant(setting.ParticipantId, User.GetId())) return Forbid();

            var result = _notificationService.UpdateNotificationSetting(setting.ParticipantId, setting.Type,
                setting.Sms, setting.Email, setting.Push);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }

        #endregion
    }
}
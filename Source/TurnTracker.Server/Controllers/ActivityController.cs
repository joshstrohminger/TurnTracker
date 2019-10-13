﻿using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ActivityController : Controller
    {
        private readonly ITurnService _turnService;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        public ActivityController(ITurnService turnService, IResourceAuthorizationService resourceAuthorizationService)
        {
            _turnService = turnService;
            _resourceAuthorizationService = resourceAuthorizationService;
        }

        [HttpGet("{id}")]
        public IActionResult GetActivityDetails(int id)
        {
            if (id <= 0) return BadRequest("ID must be greater than zero");

            var myId = User.GetId();
            if (!_resourceAuthorizationService.IsParticipantOf(id, myId)) return Forbid();

            var details = _turnService.GetActivityDetailsShallow(id, myId);
            if (details is null) return StatusCode(500);

            return Json(details);
        }

        // ReSharper disable once StringLiteralTypo
        [HttpGet("{id}/allturns")]
        public IActionResult GetActivityDetailsWithAllTurns(int id)
        {
            if (id <= 0) return BadRequest("ID must be greater than zero");

            var myId = User.GetId();
            if (!_resourceAuthorizationService.IsParticipantOf(id, myId)) return Forbid();

            var details = _turnService.GetActivityDetails(id, myId);
            if (details is null) return BadRequest();

            return Json(details);
        }

        [HttpGet("{id}/edit")]
        public IActionResult GetActivityForEdit(int id)
        {
            if (id <= 0) return BadRequest("ID must be greater than zero");

            var myId = User.GetId();
            if (!_resourceAuthorizationService.CanModifyActivity(id, myId)) return Forbid();

            var activity = _turnService.GetActivityForEdit(id);
            if (activity is null) return BadRequest();

            return Json(activity);
        }

        [HttpDelete("{id}")]
        public IActionResult DisableActivity(int id)
        {
            return SetActivityDisabled(id, true);
        }

        [HttpPut("{id}")]
        public IActionResult EnableActivity(int id)
        {
            return SetActivityDisabled(id, false);
        }

        private IActionResult SetActivityDisabled(int id, bool disabled)
        {
            if (id <= 0) return BadRequest("ID must be greater than zero");

            var myId = User.GetId();
            if (!_resourceAuthorizationService.IsOwnerOf(id, myId)) return Forbid();

            var result = _turnService.SetActivityDisabled(id, disabled);
            if (result.IsSuccess) return Ok();

            return StatusCode(500);
        }
    }
}
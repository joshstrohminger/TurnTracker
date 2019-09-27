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
    }
}
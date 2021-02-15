using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TurnController : Controller
    {
        private readonly ITurnService _turnService;
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        public TurnController(ITurnService turnService, IResourceAuthorizationService resourceAuthorizationService)
        {
            _turnService = turnService;
            _resourceAuthorizationService = resourceAuthorizationService;
        }

        [HttpPost]
        public IActionResult TakeTurn([FromBody] NewTurn turn)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var myId = User.GetId();
            var authResult = _resourceAuthorizationService.CanTakeTurn(turn.ActivityId, myId, turn.ForUserId);
            if (authResult.IsFailure)
            {
                return StatusCode(403, authResult.Error);
            }

            var result = _turnService.TakeTurn(turn.ModifiedDate, turn.ActivityId, myId, turn.ForUserId, turn.When);
            if (result.IsSuccess)
            {
                return Json(result.Value);
            }
            
            return result.Error switch
            {
                TurnError.ActivityModified => StatusCode((int) HttpStatusCode.Conflict),
                _ => StatusCode(500)
            };
        }

        [HttpDelete("{id}")]
        public IActionResult DisableTurn(int id)
        {
            return SetTurnDisabled(id, true);
        }

        [HttpPut("{id}")]
        public IActionResult EnableTurn(int id)
        {
            return SetTurnDisabled(id, false);
        }

        private IActionResult SetTurnDisabled(int id, bool disabled)
        {
            if (id <= 0) return BadRequest("ID must be greater than zero");

            var myId = User.GetId();
            if (!_resourceAuthorizationService.CanModifyTurn(id, myId)) return Forbid();

            var result = _turnService.SetTurnDisabled(id, User.GetId(), disabled);
            if (result.IsSuccess) return Json(result.Value);

            return StatusCode(500);
        }
    }
}
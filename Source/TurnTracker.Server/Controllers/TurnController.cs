using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TurnController : Controller
    {
        private readonly ITurnService _turnService;

        public TurnController(ITurnService turnService)
        {
            _turnService = turnService;
        }

        [HttpGet("{id}")]
        public IActionResult GetTurn(int id)
        {
            var turn = _turnService.GetTurn(id);
            if (turn is null) return NotFound();
            return Json(turn);
        }

        [HttpPost]
        public IActionResult TakeTurn([FromBody] NewTurn turn)
        {
            var myId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = _turnService.TakeTurn(turn.ActivityId, myId, turn.ForUserId, turn.When);
            if (result.IsSuccess) return Ok();

            return BadRequest();
        }

        [HttpDelete("{id}")]
        public IActionResult DisableTurn(int id)
        {
            var result = _turnService.DisableTurn(id, int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            if (result.IsSuccess) return Ok();

            return BadRequest();
        }
    }
}
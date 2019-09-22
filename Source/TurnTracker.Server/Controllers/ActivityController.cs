using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ActivityController : Controller
    {
        private readonly ITurnService _turnService;

        public ActivityController(ITurnService turnService)
        {
            _turnService = turnService;
        }

        [HttpGet("{id}")]
        public IActionResult GetActivityDetails(int id)
        {
            //todo verify that the user has access to this
            var details = _turnService.GetActivityDetailsShallow(id);
            if (details is null) return BadRequest();

            return Json(details);
        }

        // ReSharper disable once StringLiteralTypo
        [HttpGet("{id}/allturns")]
        public IActionResult GetActivityDetailsWithAllTurns(int id)
        {
            //todo verify that the user has access to this
            var details = _turnService.GetActivityDetails(id);
            if (details is null) return BadRequest();

            return Json(details);
        }
    }
}
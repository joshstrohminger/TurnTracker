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
            var activity = _turnService.GetActivityDetails(id);
            if (activity is null) return BadRequest();

            return Json(activity);
        }
    }
}
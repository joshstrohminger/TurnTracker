using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Utilities;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class SettingsController : Controller
    {
        private readonly IUserService _userService;

        public SettingsController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("[action]")]
        public IActionResult ShowDisabledActivities([FromBody] bool show)
        {
            var myId = User.GetId();
            if (_userService.SetShowDisabledActivities(myId, show).IsFailure)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
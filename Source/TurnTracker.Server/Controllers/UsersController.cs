using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetFilteredUsers([FromQuery] string filter)
        {
            var result = _userService.FindUsers(filter?.Trim());
            if (result.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(result.Value);
        }
    }
}
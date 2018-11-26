using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Data;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        private readonly IUserService _userService;

        public TestController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public string Data()
        {
            return "some data for anybody";
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Login(string username, string password)
        {
            var user = _userService.Authenticate(username, password);
            if (user is null) return Unauthorized();
            return Ok(new AuthenticatedUser(user));
        }
        
        [HttpPost("[action]")]
        public IActionResult Logout()
        {
            if (_userService.Logout(User.Identity.Name))
            {
                return Ok();
            }

            return NotFound();
        }
        
        [HttpGet("[action]")]
        public string UserData()
        {
            return $"some data only for users, current user {User.Identity.Name}";
        }
        
        [Authorize(Roles = nameof(Role.Admin))]
        [HttpGet("[action]")]
        public string AdminData()
        {
            return $"some data only for admins, current user {User.Identity.Name}";
        }
    }
}

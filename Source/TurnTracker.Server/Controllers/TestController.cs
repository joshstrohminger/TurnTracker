using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
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
            var (_, isFailure, (user, refreshToken)) = _userService.AuthenticateUser(username, password);
            if (isFailure) return Unauthorized();
            return Ok(new AuthenticatedUser(user, refreshToken));
        }
        
        [HttpPost("[action]")]
        public IActionResult Logout()
        {
            if (_userService.LogoutUser(User.Identity.Name).IsSuccess)
            {
                return Ok();
            }

            return NotFound();
        }

        [Authorize(Policy = nameof(PolicyType.Refresh))]
        [HttpPost("[action]")]
        public IActionResult Refresh()
        {
            var (isSuccess, _, accessToken) = _userService.RefreshUser(User.Identity.Name, User.FindFirstValue(nameof(ClaimType.RefreshKey)));
            if (isSuccess)
            {
                return Ok(accessToken);
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult AcceptInvite(string token)
        {
            return null;
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

using System.Security.Claims;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TurnTracker.Data;
using TurnTracker.Domain.Authorization;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Login([FromBody] Credentials credentials)
        {
            var (_, isFailure, (user, refreshToken)) = _userService.AuthenticateUser(credentials.Username, credentials.Password);
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

        [HttpGet("[action]")]
        public IActionResult Profile()
        {
            var (isSuccess, _, user) = _userService.GetUser(User.Identity.Name);
            if (isSuccess)
            {
                var profile = new UserProfile(user);
                return Ok(profile);
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult AcceptInvite(string token)
        {
            return null;
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public string AnonymousData()
        {
            return "some data for anybody";
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

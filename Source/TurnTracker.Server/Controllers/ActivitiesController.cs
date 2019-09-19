using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;

namespace TurnTracker.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ActivitiesController : Controller
    {
        private readonly ITurnService _turnService;

        public ActivitiesController(ITurnService turnService)
        {
            _turnService = turnService;
        }

        [HttpGet("participating")]
        public IActionResult GetMyActivities()
        {
            return Json(_turnService
                .GetActivitiesByParticipant(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
                .Select(a => new ActivitySummary
                {
                    Id = a.Id,
                    Name = a.Name
                }));
        }
    }
}
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Server.Models;
using TurnTracker.Server.Utilities;

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
                .GetActivitiesByParticipant(User.GetId())
                .Select(a => new ActivitySummary
                {
                    Id = a.Id,
                    Name = a.Name,
                    CurrentTurnUserId = a.CurrentTurnUserId,
                    CurrentTurnUserDisplayName = a.CurrentTurnUser?.DisplayName,
                    Due = a.Due,
                    IsDisabled = a.IsDisabled
                }));
        }
    }
}
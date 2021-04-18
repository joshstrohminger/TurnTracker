using System.Linq;
using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly ITurnService _turnService;

        public ActivitiesController(ITurnService turnService, IMapper mapper)
        {
            _turnService = turnService;
            _mapper = mapper;
        }

        [HttpGet("participating")]
        public IActionResult GetMyActivities()
        {
            return Json(_turnService
                .GetActivitiesByParticipant(User.GetId())
                .Select(activity => _mapper.Map<ActivitySummary>(activity)));
        }
    }
}
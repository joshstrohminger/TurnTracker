using AutoMapper;
using TurnTracker.Data.Entities;
using TurnTracker.Server.Models;

namespace TurnTracker.Server.Utilities
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Activity, ActivitySummary>();
        }
    }
}
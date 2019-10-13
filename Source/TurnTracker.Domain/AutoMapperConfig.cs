using AutoMapper;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Participant, UserInfo>()
                .ForMember(x => x.Id, x => x.MapFrom(y => y.User.Id))
                .ForMember(x => x.Name, x => x.MapFrom(y => y.User.DisplayName));

            CreateMap<Activity, EditableActivity>();
        }
    }
}
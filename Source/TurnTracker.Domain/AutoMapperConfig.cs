using AutoMapper;
using Fido2NetLib;
using Lib.Net.Http.WebPush;
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
                .ForMember(x => x.Name, x => x.MapFrom(y => y.User.DisplayName))
                .ForMember(x => x.Role, o => o.Ignore());

            CreateMap<User, UserInfo>()
                .ForMember(x => x.Name, x => x.MapFrom(y => y.DisplayName));

            CreateMap<Activity, EditableActivity>();

            CreateMap<NotificationInfo, NotificationInfo>();

            CreateMap<NotificationInfo, DefaultNotificationSetting>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Activity, o => o.Ignore())
                .ForMember(x => x.ActivityId, o => o.Ignore())
                .ForMember(x => x.CreatedDate, o => o.Ignore())
                .ForMember(x => x.ModifiedDate, o => o.Ignore())
                .ForMember(x => x.Timestamp, o => o.Ignore())
                .ReverseMap();

            CreateMap<NotificationInfo, NotificationSetting>()
                .ForMember(x => x.Id, o => o.Ignore())
                .ForMember(x => x.Origin, o => o.Ignore())
                .ForMember(x => x.NextCheck, o => o.Ignore())
                .ForMember(x => x.Participant, o => o.Ignore())
                .ForMember(x => x.CreatedDate, o => o.Ignore())
                .ForMember(x => x.ModifiedDate, o => o.Ignore())
                .ForMember(x => x.Timestamp, o => o.Ignore())
                .ReverseMap();

            CreateMap<PushSubscription, PushSubscriptionDevice>()
                .ForMember(x => x.UserId, o => o.Ignore())
                .ForMember(x => x.User, o => o.Ignore())
                .ForMember(x => x.CreatedDate, o => o.Ignore())
                .ForMember(x => x.ModifiedDate, o => o.Ignore())
                .ForMember(x => x.Timestamp, o => o.Ignore())
                .ReverseMap();

            CreateMap<AssertionOptions, AnonymousAssertionOptions>()
                .ForMember(x => x.RequestId, o => o.Ignore());
        }
    }
}
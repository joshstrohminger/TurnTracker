using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class ParticipantInfo
    {
        public int Id { get; }
        public int UserId { get; }
        public string Name { get; }
        public int TurnsNeeded { get; }
        public bool HasDisabledTurns { get; }
        public int TurnOrder { get; }
        public List<NotificationInfo> NotificationSettings { get; }
        public bool? HasEmail { get; set; }
        public bool? HasMobileNumber { get; set; }
        public TimeSpan? DismissTimeOfDay { get; set; }

        internal ParticipantInfo(TurnCount turnCount, int mostTurnsTaken, int turnOrder, bool includeNotificationSettings, IMapper mapper) : this(turnCount.Participant, includeNotificationSettings, mapper)
        {
            TurnsNeeded = mostTurnsTaken - turnCount.Count;
            HasDisabledTurns = turnCount.HasDisabledTurns;
            TurnOrder = turnOrder;

            // Copy the changes to the source object in case they get persisted
            turnCount.Participant.TurnsNeeded = TurnsNeeded;
            turnCount.Participant.HasDisabledTurns = HasDisabledTurns;
            turnCount.Participant.TurnOrder = TurnOrder;
        }

        internal ParticipantInfo(Participant p, bool includeNotificationSettings, IMapper mapper)
        {
            Id = p.Id;
            UserId = p.UserId;
            Name = p.User.DisplayName;
            TurnsNeeded = p.TurnsNeeded;
            HasDisabledTurns = p.HasDisabledTurns;
            TurnOrder = p.TurnOrder;
            if (includeNotificationSettings && mapper != null)
            {
                NotificationSettings = mapper.Map<List<NotificationInfo>>(p.NotificationSettings);
                HasEmail = p.User.Email != null;
                HasMobileNumber = p.User.MobileNumber != null;
                DismissTimeOfDay = p.DismissUntilTimeOfDay;
            }
        }
    }
}
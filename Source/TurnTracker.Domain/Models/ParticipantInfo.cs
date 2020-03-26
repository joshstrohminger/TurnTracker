using System;
using System.Collections.Generic;
using System.Linq;
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

        internal ParticipantInfo(TurnCount turnCount, int mostTurnsTaken, int turnOrder, bool includeNotificationSettings) : this(turnCount.Participant, includeNotificationSettings)
        {
            TurnsNeeded = mostTurnsTaken - turnCount.Count;
            HasDisabledTurns = turnCount.HasDisabledTurns;
            TurnOrder = turnOrder;

            // Copy the changes to the source object in case they get persisted
            var p = turnCount.Participant;
            p.TurnsNeeded = TurnsNeeded;
            p.HasDisabledTurns = HasDisabledTurns;
            p.TurnOrder = TurnOrder;
        }

        internal ParticipantInfo(Participant p, bool includeNotificationSettings)
        {
            Id = p.Id;
            UserId = p.UserId;
            Name = p.User.DisplayName;
            TurnsNeeded = p.TurnsNeeded;
            HasDisabledTurns = p.HasDisabledTurns;
            TurnOrder = p.TurnOrder;
            if (includeNotificationSettings)
            {
                NotificationSettings = p.NotificationSettings.Select(x => new NotificationInfo(x)).ToList();
                HasEmail = p.User.Email != null;
                HasMobileNumber = p.User.MobileNumber != null;
                DismissTimeOfDay = p.DismissUntilTimeOfDay;
            }
        }
    }
}
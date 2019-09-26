using System;
using System.Collections.Generic;
using System.Linq;
using TurnTracker.Data;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class ActivityDetails
    {
        public int Id { get; }
        public string Name { get; }
        public bool HasDisabledTurns { get; }
        public DateTimeOffset? Due { get; }
        public Unit? PeriodUnit { get; }
        public uint? PeriodCount { get; }
        public string OwnerName { get; }
        public int? CurrentTurnUserId { get; }
        public string CurrentTurnUserDisplayName { get; }
        public List<ParticipantInfo> Participants { get; }
        public List<TurnInfo> Turns { get; }

        public static ActivityDetails Calculate(Activity activity, int userId)
        {
            return new ActivityDetails(activity, true, userId);
        }

        public static ActivityDetails Populate(Activity activity, int userId)
        {
            return new ActivityDetails(activity, false, userId);
        }

        private ActivityDetails(Activity activity, bool calculate, int userId)
        {
            Id = activity.Id;
            Name = activity.Name;
            PeriodUnit = activity.PeriodUnit;
            PeriodCount = activity.PeriodCount;
            OwnerName = activity.Owner.DisplayName;

            if (calculate)
            {
                var counts = activity.Participants.ToDictionary(x => x.UserId, x => new TurnCount(x));
                Turns = activity.Turns.OrderByDescending(x => x.Occurred).Select(turn =>
                {
                    var info = new TurnInfo(turn);
                    if (counts.TryGetValue(turn.UserId, out var turnCount))
                    {
                        turnCount.Add(turn);
                    }

                    return info;
                }).ToList();

                if (activity.Period.HasValue)
                {
                    if (Turns.Count == 0)
                    {
                        Due = DateTimeOffset.Now;
                    }
                    else
                    {
                        Due = Turns[0].Occurred + activity.Period.Value;
                    }
                }

                var mostTurnsTaken = counts.Values.Max(x => x.Count);

                Participants = counts.Values
                    .OrderBy(x => x.Count)
                    .ThenBy(x => x.FirstTurn)
                    .ThenBy(x => x.Participant.Id)
                    .Select((x,i) => new ParticipantInfo(x, mostTurnsTaken, i, x.Participant.UserId == userId))
                    .ToList();

                HasDisabledTurns = Participants.Any(x => x.HasDisabledTurns);

                if (activity.TakeTurns && Participants.Count > 0)
                {
                    CurrentTurnUserId = Participants[0].UserId;
                    CurrentTurnUserDisplayName = Participants[0].Name;
                }

                // Copy the calculated values to the source object in case they're different and they get persisted
                activity.Due = Due;
                activity.CurrentTurnUserId = CurrentTurnUserId;
                activity.HasDisabledTurns = HasDisabledTurns;
            }
            else
            {
                // just copy over values since we aren't calculating them
                HasDisabledTurns = activity.HasDisabledTurns;
                CurrentTurnUserId = activity.CurrentTurnUserId;
                CurrentTurnUserDisplayName = activity.CurrentTurnUser?.DisplayName;
                Due = activity.Due;
                Participants = activity.Participants
                    .OrderBy(x => x.TurnOrder)
                    .Select(x => new ParticipantInfo(x, x.UserId == userId))
                    .ToList();
            }
        }
    }
}
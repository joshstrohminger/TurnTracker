﻿using System;
using System.Collections.Generic;
using System.Linq;
using TurnTracker.Data;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class ActivityInfo
    {
        public int Id { get; }
        public string Name { get; }
        public bool HasDisabledTurns { get; }
        public DateTimeOffset? Due { get; }
        public TimeSpan? Period { get; }
        public Unit? PeriodUnit { get; }
        public uint? PeriodCount { get; }
        public string OwnerName { get; }
        public List<ParticipantInfo> Participants { get; }
        public List<TurnInfo> Turns { get; }

        public ActivityInfo(Activity activity)
        {
            Id = activity.Id;
            Name = activity.Name;
            Period = activity.Period;
            PeriodUnit = activity.PeriodUnit;
            PeriodCount = activity.PeriodCount;
            OwnerName = activity.Owner.DisplayName;

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

            if (Period.HasValue)
            {
                if (Turns.Count == 0)
                {
                    Due = DateTimeOffset.UtcNow;
                }
                else
                {
                    Due = Turns[0].Occurred + Period.Value;
                }
            }

            var mostTurnsTaken = counts.Values.Max(x => x.Count);

            Participants = counts.Values
                .OrderBy(x => x.Count)
                .ThenBy(x => x.FirstTurn)
                .ThenBy(x => x.Participant.Id)
                .Select(x => new ParticipantInfo(x, mostTurnsTaken))
                .ToList();

            HasDisabledTurns = Participants.Any(x => x.HasDisabledTurns);
        }
    }
}
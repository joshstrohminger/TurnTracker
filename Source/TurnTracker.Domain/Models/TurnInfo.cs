using System;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class TurnInfo
    {
        public int Id { get; }
        public int UserId { get; }
        public DateTimeOffset Occurred { get; }
        public int CreatorId { get; }
        public bool IsDisabled { get; }

        public TurnInfo(Turn turn)
        {
            Id = turn.Id;
            UserId = turn.UserId;
            Occurred = turn.Occurred;
            CreatorId = turn.CreatorId;
            IsDisabled = turn.IsDisabled;
        }
    }
}
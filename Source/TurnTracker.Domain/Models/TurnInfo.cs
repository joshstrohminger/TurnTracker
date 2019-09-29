using System;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class TurnInfo
    {
        public int Id { get; }
        public int UserId { get; }
        public DateTimeOffset Occurred { get; }
        public DateTimeOffset Created { get; }
        public int CreatorId { get; }
        public bool IsDisabled { get; }
        public int? ModifierId { get; }
        public DateTimeOffset Modified { get; }

        public TurnInfo(Turn turn)
        {
            Id = turn.Id;
            UserId = turn.UserId;
            Occurred = turn.Occurred;
            Created = turn.CreatedDate;
            CreatorId = turn.CreatorId;
            IsDisabled = turn.IsDisabled;
            ModifierId = turn.ModifierId;
            Modified = turn.ModifiedDate;
        }
    }
}
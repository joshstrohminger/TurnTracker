using System;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    public class TurnInfo
    {
        private Turn _turn;
        private int _id;

        public int Id
        {
            get
            {
                if (_id == 0 && _turn != null && _turn.Id != 0)
                {
                    // The turn has been saved and an updated ID is available
                    _id = _turn.Id;
                    _turn = null;
                }

                return _id;
            }
        }
        public int UserId { get; }
        public DateTimeOffset Occurred { get; }
        public DateTimeOffset Created { get; }
        public int CreatorId { get; }
        public bool IsDisabled { get; }
        public int? ModifierId { get; }
        public DateTimeOffset Modified { get; }

        public TurnInfo(Turn turn)
        {
            _id = turn.Id;
            if (_id == 0)
            {
                // keep track of the turn so it can be used to retrieve an updated id once it's been saved
                _turn = turn;
            }
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
using System;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Models
{
    internal class TurnCount
    {
        public int Count { get; private set; }
        public DateTimeOffset FirstTurn { get; private set; }
        public bool HasDisabledTurns { get; private set; }
        public Participant Participant { get; }

        public TurnCount(Participant participant)
        {
            Participant = participant;
        }

        /// <summary>
        /// Adds another turn to count. This assumes turns will only be added from newest to oldest, otherwise a date comparison will be necessary.
        /// </summary>
        public void Add(Turn turn)
        {
            if (turn.IsDisabled)
            {
                HasDisabledTurns = true;
            }
            else
            {
                FirstTurn = turn.Occurred;
                Count++;
            }
        }
    }
}
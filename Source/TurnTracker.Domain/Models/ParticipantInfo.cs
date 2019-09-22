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

        internal ParticipantInfo(TurnCount turnCount, int mostTurnsTaken, int turnOrder) : this(turnCount.Participant)
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

        internal ParticipantInfo(Participant p)
        {
            Id = p.Id;
            UserId = p.UserId;
            Name = p.User.DisplayName;
            TurnsNeeded = p.TurnsNeeded;
            HasDisabledTurns = p.HasDisabledTurns;
            TurnOrder = p.TurnOrder;
        }
    }
}
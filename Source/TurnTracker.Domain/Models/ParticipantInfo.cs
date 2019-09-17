namespace TurnTracker.Domain.Models
{
    public class ParticipantInfo
    {
        public int Id { get; }
        public string Name { get; }
        public int TurnsNeeded { get; }
        public bool HasDisabledTurns { get; }

        internal ParticipantInfo(TurnCount turnCount, int mostTurnsTaken)
        {
            Id = turnCount.Participant.Id;
            Name = turnCount.Participant.User.DisplayName;
            TurnsNeeded = mostTurnsTaken - turnCount.Count;
            HasDisabledTurns = turnCount.HasDisabledTurns;
        }
    }
}
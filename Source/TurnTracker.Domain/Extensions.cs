using System;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain
{
    public static class Extensions
    {
        public static bool IsAllowed(this NotificationType type, bool takeTurns)
        {
            return type switch
            {
                NotificationType.TurnTakenMine => takeTurns,
                NotificationType.OverdueMine => takeTurns,
                NotificationType.TurnTakenAnybody => true,
                NotificationType.OverdueAnybody => true,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unexpected type")
            };
        }
    }
}
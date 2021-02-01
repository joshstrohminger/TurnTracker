using TurnTracker.Data.Entities;

namespace TurnTracker.Domain
{
    public static class Extensions
    {
        public static bool IsAllowed(this NotificationType type, bool takeTurns)
        {
            switch (type)
            {
                case NotificationType.TurnTakenMine:
                case NotificationType.OverdueMine:
                    return takeTurns;
                default:
                    return true;
            }
        }
    }
}
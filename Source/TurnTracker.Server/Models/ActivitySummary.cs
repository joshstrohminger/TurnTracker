using System;

namespace TurnTracker.Server.Models
{
    public class ActivitySummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? CurrentTurnUserId { get; set; }
        public string CurrentTurnUserDisplayName { get; set; }
        public DateTimeOffset? Due { get; set; }
        public bool IsDisabled { get; set; }
    }
}
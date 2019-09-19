using System;

namespace TurnTracker.Server.Models
{
    public class NewTurn
    {
        public int ActivityId { get; set; }
        public int ByUserId { get; set; }
        public int ForUserId { get; set; }
        public DateTimeOffset When { get; set; }
    }
}
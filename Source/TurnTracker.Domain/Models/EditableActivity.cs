using System.Collections.Generic;
using TurnTracker.Data;

namespace TurnTracker.Domain.Models
{
    public class EditableActivity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDisabled { get; set; }
        public Unit? PeriodUnit { get; set; }
        public uint? PeriodCount { get; set; }
        public bool TakeTurns { get; set; }
        public List<UserInfo> Participants { get; set; }
    }
}
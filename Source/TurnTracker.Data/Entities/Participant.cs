using System;
using System.Collections.Generic;
using System.Text;

namespace TurnTracker.Data.Entities
{
    public class Participant : Entity
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ActivityId { get; set; }
        public Activity Activity { get; set; }
    }
}

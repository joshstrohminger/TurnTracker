using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TurnTracker.Data.Entities
{
    public class Activity : Entity
    {
        [Required]
        public string Name { get; set; }

        public TimeSpan? Period { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public List<Participant> Participants { get; set; }
        public List<Turn> Turns { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public class Activity : Entity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType("interval")]
        public TimeSpan? Period { get; set; }
        public Unit? PeriodUnit { get; set; }
        public uint? PeriodCount { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public List<Participant> Participants { get; set; }
        public List<Turn> Turns { get; set; }
    }
}

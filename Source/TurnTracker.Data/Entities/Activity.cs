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
        public bool TakeTurns { get; set; }

        public int OwnerId { get; set; }
        public User Owner { get; set; }

        public List<Participant> Participants { get; set; }
        public List<Turn> Turns { get; set; }

        #region Calculated Values

        /// <summary>
        /// This will be <c>null</c> if the activity is configured not to take turns.
        /// </summary>
        public int? CurrentTurnUserId { get; set; }
        public User CurrentTurnUser { get; set; }

        /// <summary>
        /// This will be <c>null</c> if the activity is configured to not be periodic
        /// </summary>
        public DateTimeOffset? Due { get; set; }

        public bool HasDisabledTurns { get; set; }

        #endregion
    }
}

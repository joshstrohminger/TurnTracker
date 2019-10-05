using System;
using System.ComponentModel.DataAnnotations;
using TurnTracker.Server.Validators;

namespace TurnTracker.Server.Models
{
    public class NewTurn
    {
        [Range(1, int.MaxValue)]
        public int ActivityId { get; set; }

        [Range(1, int.MaxValue)]
        public int ForUserId { get; set; }

        [InThePast]
        public DateTimeOffset When { get; set; }
    }
}
using System;

namespace TurnTracker.Domain.Models
{
    public class Session
    {
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public long Id { get; set; }
        public bool Current { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace TurnTracker.Domain.Models
{
    public class Device
    {
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public int Id { get; set; }
        public bool Current { get; set; }
        public IEnumerable<Session> Sessions { get; set; }
    }
}
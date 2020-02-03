using System;
using System.ComponentModel.DataAnnotations;

namespace TurnTracker.Data.Entities
{
    public abstract class Entity
    {
        public DateTimeOffset CreatedDate { get; set; }

        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// This is used by the database/efcore to determine if there is a concurrency issue
        /// </summary>
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}

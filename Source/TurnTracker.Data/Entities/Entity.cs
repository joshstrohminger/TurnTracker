using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public abstract class Entity
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset ModifiedDate { get; set; }

        /// <summary>
        /// This is used by the database/efcore to determine if there is a concurrency issue
        /// </summary>
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}

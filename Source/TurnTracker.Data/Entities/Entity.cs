using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TurnTracker.Data.Entities
{
    public abstract class Entity
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedDate { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset ModifiedDate { get; set; }
    }
}

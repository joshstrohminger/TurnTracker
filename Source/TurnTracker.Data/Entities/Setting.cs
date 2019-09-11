using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text;

namespace TurnTracker.Data.Entities
{
    public class Setting : Entity
    {
        [Key]
        public string Key { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }
        
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public bool BoolValue { get; set; }
    }
}

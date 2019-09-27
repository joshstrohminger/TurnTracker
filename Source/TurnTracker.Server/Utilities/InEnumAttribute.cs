using System;
using System.ComponentModel.DataAnnotations;
using TurnTracker.Common;

namespace TurnTracker.Server.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InEnumAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return validationContext.ObjectType.IsEnum &&
                   Enum.IsDefined(validationContext.ObjectType, value)
                ? ValidationResult.Success
                : new ValidationResult("Must be a valid enum value", validationContext.DisplayName.Yield());
        }
    }
}
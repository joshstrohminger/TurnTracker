using System;
using System.ComponentModel.DataAnnotations;
using TurnTracker.Common;

namespace TurnTracker.Server.Utilities
{
    [AttributeUsage(AttributeTargets.Property)]
    public class InThePastAttribute : ValidationAttribute
    {
        public TimeSpan ClockSkew { get; }

        public InThePastAttribute()
        {
            ClockSkew = TimeSpan.FromMinutes(5);
        }

        public InThePastAttribute(TimeSpan clockSkew)
        {
            ClockSkew = clockSkew;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            switch (value)
            {
                case DateTimeOffset dto:
                    if (dto < DateTimeOffset.Now + ClockSkew)
                    {
                        return ValidationResult.Success;
                    }
                    break;
                case DateTime dt:
                    if (dt < DateTime.Now + ClockSkew)
                    {
                        return ValidationResult.Success;
                    }
                    break;
                default:
                    return new ValidationResult("Unsupported type", validationContext.DisplayName.Yield());
            }
            return new ValidationResult("Date may not be in the future", validationContext.DisplayName.Yield());
        }
    }
}
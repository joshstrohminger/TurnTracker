using System;
using System.ComponentModel.DataAnnotations;
using TurnTracker.Common;

namespace TurnTracker.Server.Validators
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MinTrimmedLengthAttribute : ValidationAttribute
    {
        public int Min { get; }

        public MinTrimmedLengthAttribute(int min)
        {
            if(min <= 0) throw new ArgumentOutOfRangeException(nameof(min), "Must be greater than zero");
            Min = min;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string s)
            {
                if (s.Trim().Length >= Min)
                {
                    return ValidationResult.Success;
                }
                return new ValidationResult($"Min Character Length: {Min}", validationContext.DisplayName.Yield());
            }
            return new ValidationResult("Unsupported type", validationContext.DisplayName.Yield());
        }
    }
}
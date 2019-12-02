using System;
using System.Net.Http.Headers;
using CSharpFunctionalExtensions;

namespace TurnTracker.Common
{
    public class ValidityError
    {
        public string Message { get; }
        public bool Invalid { get; }

        private ValidityError(bool invalid, string message)
        {
            Invalid = invalid;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public static Result<TValue, ValidityError> ForInvalidObject<TValue>(string message)
        {
            return Result.Failure<TValue, ValidityError>(new ValidityError(true, message));
        }

        public static Result<TValue, ValidityError> ForValidObject<TValue>(string message)
        {
            return Result.Failure<TValue, ValidityError>(new ValidityError(false, message));
        }
    }
}
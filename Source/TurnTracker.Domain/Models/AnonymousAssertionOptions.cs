using Fido2NetLib;

namespace TurnTracker.Domain.Models
{
    public class AnonymousAssertionOptions : AssertionOptions
    {
        public string RequestId { get; set; }
    }
}
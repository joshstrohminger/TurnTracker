using Fido2NetLib;

namespace TurnTracker.Domain.Models
{
    public class AnonymousAuthenticatorAssertionRawResponse : AuthenticatorAssertionRawResponse
    {
        public string RequestId { get; set; }
    }
}
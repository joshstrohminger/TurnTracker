using Fido2NetLib;

namespace TurnTracker.Server.Models
{
    public class AuthenticatorAttestationNamedRawResponse : AuthenticatorAttestationRawResponse
    {
        public string DeviceName { get; set; }
    }
}
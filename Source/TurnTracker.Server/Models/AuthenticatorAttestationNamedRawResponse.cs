using Fido2NetLib;

namespace TurnTracker.Server.Models
{
    public class AuthenticatorAttestationNamedRawResponse
    {
        public string DeviceName { get; set; }
        public AuthenticatorAttestationRawResponse RawResponse { get; set; }
    }
}
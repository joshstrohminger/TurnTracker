using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fido2NetLib;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public interface IWebAuthnService
    {
        Result<CredentialCreateOptions> MakeCredentialOptions(int userId, string username, string displayName, long loginId);
        Task<Result<Fido2.CredentialMakeResult>> MakeCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, int userId, long loginId, string deviceName);
        Result<AssertionOptions> MakeAssertionOptions(string username);
        Task<Result<(User user, string accessToken, string refreshToken), (bool unauthorized, string message)>>
            MakeAssertionAsync(AnonymousAuthenticatorAssertionRawResponse clientResponse, string deviceName);
    }
}
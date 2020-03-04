using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fido2NetLib;

namespace TurnTracker.Domain.Interfaces
{
    public interface IWebAuthnService
    {
        Result<CredentialCreateOptions> MakeCredentialOptions(int userId, string username, string displayName, long loginId);
        Task<Result<Fido2.CredentialMakeResult>> MakeCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, int userId, long loginId);
        Result<AssertionOptions> MakeAssertionOptions(int userId);
        Task<Result> MakeAssertionAsync(int userId, AuthenticatorAssertionRawResponse clientResponse);
    }
}
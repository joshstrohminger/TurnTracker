using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class WebAuthnService : IWebAuthnService
    {
        #region Fields

        private readonly ILogger<WebAuthnService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IFido2 _fido2;
        private readonly IOptions<AppSettings> _options;
        private readonly TurnContext _db;

        #endregion Fields

        #region Ctor

        public WebAuthnService(ILogger<WebAuthnService> logger, IMemoryCache cache, IFido2 fido2, IOptions<AppSettings> options, TurnContext db)
        {
            _logger = logger;
            _cache = cache;
            _fido2 = fido2;
            _options = options;
            _db = db;
        }

        #endregion Ctor

        #region Public

        public Result<CredentialCreateOptions> MakeCredentialOptions(int userId, string username, string displayName, long loginId)
        {
            var user = new Fido2User
            {
                Name = username,
                DisplayName = displayName,
                Id = BitConverter.GetBytes(userId)
            };

            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = true,
                UserVerification = UserVerificationRequirement.Required
            };

            var extensions = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                UserVerificationIndex = true,
                Location = true,
                UserVerificationMethod = true,
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                {
                    FAR = float.MaxValue,
                    FRR = float.MaxValue
                }
            };

            var options = _fido2.RequestNewCredential(user, null, authenticatorSelection, AttestationConveyancePreference.Direct, extensions);

            _cache.Set($"CredentialOptions:{loginId}", options, _options.Value.ChallengeExpiration);

            _logger.LogInformation($"Created options for login {loginId}");

            return Result.Success(options);
        }

        public async Task<Result<Fido2.CredentialMakeResult>> MakeCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, int userId, long loginId)
        {
            try
            {
                // 1. get the options we sent the client
                if (!_cache.TryGetValue($"CredentialOptions:{loginId}", out CredentialCreateOptions options))
                {
                    _logger.LogError($"Failed to find credential options for user {userId} login {loginId}");
                    return Result.Failure<Fido2.CredentialMakeResult>("No challenge found");
                }

                // 2. Verify and make the credentials
                var cmr = await _fido2.MakeNewCredentialAsync(attestationResponse, options,
                    x => Task.FromResult(true));

                // 3. Store the credentials in db
                _db.DeviceAuthorizations.Add(new DeviceAuthorization
                {
                    UserId = userId,
                    PublicKey = cmr.Result.PublicKey,
                    CredentialId = cmr.Result.CredentialId,
                    SignatureCounter = cmr.Result.Counter
                });
                await _db.SaveChangesAsync();

                // 4. return "ok" to the client
                _logger.LogInformation($"Created credential for user {userId} login {loginId}");
                return Result.Success(cmr);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to make credential for user {userId} login {loginId}");
                return Result.Failure<Fido2.CredentialMakeResult>("Error making credential");
            }
        }

        public Result<AssertionOptions> MakeAssertionOptions(int userId)
        {
            try
            {
                // 1. Get registered credentials from database
                var existingAuthorizations = _db.DeviceAuthorizations
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    //.Select(x => new PublicKeyCredentialDescriptor(x.CredentialId))
                    .ToList();

                if (existingAuthorizations.Count == 0)
                {
                    return Result.Failure<AssertionOptions>("No existing credentials");
                }

                var mismatched = false;
                foreach (var mismatch in existingAuthorizations.Where(x => x.UserId != userId))
                {
                    mismatched = true;
                    _logger.LogError($"User {userId} attempted to make assertion options for user {mismatch.UserId}");
                }

                if (mismatched)
                {
                    return Result.Failure<AssertionOptions>("Invalid credentials");
                }

                // 2. Create options
                var extensions = new AuthenticationExtensionsClientInputs()
                {
                    SimpleTransactionAuthorization = "FIDO",
                    GenericTransactionAuthorization = new TxAuthGenericArg
                    {
                        ContentType = "text/plain",
                        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                    },
                    UserVerificationIndex = true,
                    Location = true,
                    UserVerificationMethod = true
                };
                var options = _fido2.GetAssertionOptions(
                    existingAuthorizations.Select(x => new PublicKeyCredentialDescriptor(x.CredentialId)),
                    UserVerificationRequirement.Preferred,
                    extensions
                );

                // 3. Temporarily store options
                _cache.Set($"AssertionOptions:{userId}", options, _options.Value.ChallengeExpiration);

                // 4. Return options to client
                return Result.Success(options);
            }

            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to make assertion options for user {userId}");
                return Result.Failure<AssertionOptions>("Error starting assertion");
            }
        }

        public async Task<Result> MakeAssertionAsync(int userId, AuthenticatorAssertionRawResponse clientResponse)
        {
            try
            {
                // 1. Get the assertion options we sent the client
                if (!_cache.TryGetValue($"AssertionOptions:{userId}", out AssertionOptions options))
                {
                    _logger.LogError($"Failed to find assertion options for user {userId}");
                    return Result.Failure("No assertion found");
                }

                // 2. Get registered credential from database
                var authorization = await _db.DeviceAuthorizations
                    .FirstOrDefaultAsync(x => x.CredentialId.SequenceEqual(clientResponse.Id));

                if (authorization is null)
                {
                    _logger.LogError($"Failed to find credentials for user {userId}");
                    return Result.Failure("No credentials found");
                }

                if (authorization.UserId != userId)
                {
                    _logger.LogError($"User {userId} attempted to make assertion with credentials for user {authorization.UserId}");
                    return Result.Failure("Invalid credentials");
                }

                // 3. Make the assertion
                var avr = await _fido2.MakeAssertionAsync(clientResponse, options, authorization.PublicKey, authorization.SignatureCounter, x => Task.FromResult(true));

                // 4. Store the updated counter
                authorization.SignatureCounter = avr.Counter;
                await _db.SaveChangesAsync();

                // 5. return OK to client
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to make assertion for user {userId}");
                return Result.Failure("Error making assertion");
            }
        }

        #endregion Public
    }
}
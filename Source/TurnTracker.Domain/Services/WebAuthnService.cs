using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using TurnTracker.Domain.Models;

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
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        #endregion Fields

        #region Ctor

        public WebAuthnService(ILogger<WebAuthnService> logger, IMemoryCache cache, IFido2 fido2, IOptions<AppSettings> options, TurnContext db, IMapper mapper, IUserService userService)
        {
            _logger = logger;
            _cache = cache;
            _fido2 = fido2;
            _options = options;
            _db = db;
            _mapper = mapper;
            _userService = userService;
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
                UserVerification = UserVerificationRequirement.Required
            };

            //var extensions = new AuthenticationExtensionsClientInputs
            //{
            //    Extensions = true,
            //    UserVerificationIndex = true,
            //    Location = true,
            //    UserVerificationMethod = true,
            //    BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
            //    {
            //        FAR = float.MaxValue,
            //        FRR = float.MaxValue
            //    }
            //};

            var options = _fido2.RequestNewCredential(user, null, authenticatorSelection, AttestationConveyancePreference.Direct);

            _cache.Set($"CredentialOptions:{loginId}", options, _options.Value.ChallengeExpiration);

            _logger.LogInformation($"Created options for login {loginId}");

            return Result.Success(options);
        }

        public async Task<Result<Fido2.CredentialMakeResult>> MakeCredentialAsync(AuthenticatorAttestationRawResponse attestationResponse, int userId, long loginId, string deviceName)
        {
            try
            {
                // 1. get the options we sent the client
                var cacheKey = $"CredentialOptions:{loginId}";
                if (!_cache.TryGetValue(cacheKey, out CredentialCreateOptions options))
                {
                    _logger.LogError($"Failed to find credential options for user {userId} login {loginId}");
                    return Result.Failure<Fido2.CredentialMakeResult>("No challenge found");
                }
                _cache.Remove(cacheKey);

                // 2. Verify and make the credentials
                var cmr = await _fido2.MakeNewCredentialAsync(attestationResponse, options,
                    (_, _) => Task.FromResult(true));

                // 3. Store the credentials in db
                _db.DeviceAuthorizations.Add(new DeviceAuthorization
                {
                    UserId = userId,
                    PublicKey = cmr.Result.PublicKey,
                    CredentialId = cmr.Result.CredentialId,
                    SignatureCounter = cmr.Result.Counter,
                    DeviceName = deviceName
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

        public Result<AssertionOptions> MakeAssertionOptions(string username)
        {
            try
            {
                List<DeviceAuthorization> existingAuthorizations = null;
                if (username != null)
                {
                    // 1. Get registered credentials from database
                    existingAuthorizations = _db.Users
                        .AsNoTracking()
                        .Include(x => x.DeviceAuthorizations)
                        .SingleOrDefault(x => x.Username == username)
                        ?.DeviceAuthorizations;

                    if (existingAuthorizations is null || existingAuthorizations.Count == 0)
                    {
                        return Result.Failure<AssertionOptions>("No existing credentials");
                    }
                }

                // 2. Create options
                //var extensions = new AuthenticationExtensionsClientInputs()
                //{
                //    SimpleTransactionAuthorization = "FIDO",
                //    GenericTransactionAuthorization = new TxAuthGenericArg
                //    {
                //        ContentType = "text/plain",
                //        Content = new byte[] { 0x46, 0x49, 0x44, 0x4F }
                //    },
                //    UserVerificationIndex = true,
                //    Location = true,
                //    UserVerificationMethod = true
                //}
                var options = _fido2.GetAssertionOptions(
                    existingAuthorizations?.Select(x => new PublicKeyCredentialDescriptor(x.CredentialId)),
                    UserVerificationRequirement.Preferred
                );

                // 3. Temporarily store options
                var anonymousOptions = _mapper.Map<AnonymousAssertionOptions>(options);
                anonymousOptions.RequestId = Guid.NewGuid().ToString();
                _cache.Set($"AssertionOptions:{anonymousOptions.RequestId}", options, _options.Value.ChallengeExpiration);
                options = anonymousOptions;

                // 4. Return options to client
                return Result.Success(options);
            }

            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to make assertion options for user {username}");
                return Result.Failure<AssertionOptions>("Error starting assertion");
            }
        }

        public async Task<Result<(User user, string accessToken, string refreshToken),(bool unauthorized, string message)>>
            MakeAssertionAsync(AnonymousAuthenticatorAssertionRawResponse clientResponse, string deviceName)
        {
            try
            {
                // 1. Get the assertion options we sent the client
                if (string.IsNullOrWhiteSpace(clientResponse.RequestId))
                {
                    _logger.LogError("Attempted to make assertion without request id");
                    return Result.Failure<(User, string, string), (bool, string)>((false,"No user id or request id provided"));
                }
                var cacheKey = $"AssertionOptions:{clientResponse.RequestId}";

                if (!_cache.TryGetValue(cacheKey, out AssertionOptions options))
                {
                    _logger.LogError($"Failed to find assertion options for key '{cacheKey}'");
                    return Result.Failure<(User, string, string), (bool, string)>((true,"No assertion found"));
                }
                _cache.Remove(cacheKey);

                // 2. Get registered credential from database
                var authorization = await _db.DeviceAuthorizations
                    .Include(x => x.User)
                    //todo In .net 5, ef-core 5 will support using SequenceEqual, for now we'll use normal equality and just remember that it will be properly translated into sql for real databases, see https://github.com/dotnet/efcore/issues/10582
                    //.FirstOrDefaultAsync(x => x.CredentialId.SequenceEqual(clientResponse.Id));
                    .FirstOrDefaultAsync(x => x.CredentialId == clientResponse.Id);

                if (authorization is null)
                {
                    _logger.LogError("Failed to find credentials");
                    return Result.Failure<(User, string, string), (bool, string)>((true,"No credentials found"));
                }

                // 3. Make the assertion
                var avr = await _fido2.MakeAssertionAsync(clientResponse, options, authorization.PublicKey,
                    authorization.SignatureCounter, (_, _) => Task.FromResult(true));

                // 4. Store the updated counter
                authorization.SignatureCounter = avr.Counter;

                var login = _userService.GenerateAndSaveLogin(authorization.User, deviceName, authorization.Id);
                return Result.Success<(User user, string accessToken, string refreshToken), (bool unauthorized, string message)>(login);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to make assertion for user");
                return Result.Failure<(User, string, string), (bool, string)>((false,"Error making assertion"));
            }
        }

        #endregion Public
    }
}
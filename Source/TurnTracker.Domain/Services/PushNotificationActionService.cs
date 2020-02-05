using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class PushNotificationActionService : IPushNotificationActionService
    {
        #region Fields

        private readonly TurnContext _db;
        private readonly ILogger<PushNotificationActionService> _logger;
        private readonly TimeSpan _defaultDismissTime;

        #endregion Fields

        #region Ctor

        public PushNotificationActionService(TurnContext db, ILogger<PushNotificationActionService> logger, IOptions<AppSettings> appSettings)
        {
            _db = db;
            _logger = logger;
            _defaultDismissTime = appSettings.Value.PushNotifications.DefaultDismissTime;
        }

        #endregion Ctor

        #region Public

        public async Task<Result> ActAsync(int userId, int participantId, string action)
        {
            switch (action)
            {
                case "dismiss":
                    return await DismissAsync(participantId);
                default:
                    var message = $"Invalid action notification {action} for user {userId} participant {participantId}";
                    _logger.LogError(message);
                    return Result.Failure(message);
            }
        }

        #endregion Public

        #region Priviate

        private async Task<Result> DismissAsync(int participantId)
        {
            var participant = await _db.Participants
                .Include(x => x.Activity)
                .Include(x => x.User)
                .Include(x => x.NotificationSettings)
                .SingleOrDefaultAsync(x => x.Id == participantId);

            if (participant is null)
            {
                var message = $"Invalid participant ID {participantId}";
                _logger.LogError(message);
                return Result.Failure(message);
            }

            var now = DateTimeOffset.Now;
            if (participant.Activity.Due <= now)
            {
                _logger.LogInformation($"Dismissing notification for user {participant.UserId}, participant {participant.Id}, activity {participant.ActivityId}, for {_defaultDismissTime}");
                var nextCheck = now.Add(_defaultDismissTime);

                foreach (var notificationSetting in participant.NotificationSettings.Where(x =>
                    x.Type == NotificationType.OverdueAnybody || x.Type == NotificationType.OverdueMine))
                {
                    notificationSetting.NextCheck = nextCheck;
                }

                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"Ignoring dismissal of notification for user {participant.UserId}, participant {participant.Id}, activity {participant.ActivityId}");
            }

            return Result.Success();
        }

        #endregion Private
    }
}
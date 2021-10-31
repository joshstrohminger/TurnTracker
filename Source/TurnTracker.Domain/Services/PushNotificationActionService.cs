using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class PushNotificationActionService : IPushNotificationActionService
    {
        #region Fields

        private readonly TurnContext _db;
        private readonly ILogger<PushNotificationActionService> _logger;

        #endregion Fields

        #region Ctor

        public PushNotificationActionService(TurnContext db, ILogger<PushNotificationActionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        #endregion Ctor

        #region Public

        public async Task<Result> ActAsync(int userId, int participantId, string action, DateTimeOffset clientTime)
        {
            switch (action)
            {
                case "dismiss":
                    return await SnoozeOrDismissAsync(participantId, clientTime, true);
                case "snooze":
                    return await SnoozeOrDismissAsync(participantId, clientTime, false);
                default:
                    var message = $"Invalid action notification {action} for user {userId} participant {participantId}";
                    _logger.LogError(message);
                    return Result.Failure(message);
            }
        }

        #endregion Public

        #region Private

        private async Task<Result> SnoozeOrDismissAsync(int participantId, DateTimeOffset clientTime, bool dismiss)
        {
            var typeName = dismiss ? "dismiss" : "snooze";

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

            var now = DateTimeOffset.UtcNow;
            if (participant.Activity.Due <= now)
            {
                var snoozeTime = clientTime.AddHours(Math.Max((byte)1, participant.User.SnoozeHours));
                var nextCheck = snoozeTime;

                // base calculations on the client time to have the proper timezone and to account for request delays, but check for big differences
                var diff = (now - clientTime).Duration();
                if (diff > TimeSpan.FromHours(1))
                {
                    _logger.LogWarning($"Client time {clientTime} differs from current time {now} by {diff}");
                }

                if (dismiss)
                {
                    // dismiss until the dismissal time of day on the current day in the client's timezone
                    nextCheck = new DateTimeOffset(clientTime.Date.Add(participant.DismissUntilTimeOfDay), clientTime.Offset);

                    // dismiss until tomorrow if that time has already passed or we're within the snooze time
                    if (snoozeTime >= nextCheck)
                    {
                        nextCheck = nextCheck.AddDays(1);
                    }
                }

                _logger.LogInformation($"Fulfilling {typeName} notification for user {participant.UserId}, participant {participant.Id}, activity {participant.ActivityId}, from {clientTime} to {nextCheck}");
                
                foreach (var notificationSetting in participant.NotificationSettings.Where(x =>
                    x.Type == NotificationType.OverdueAnybody || x.Type == NotificationType.OverdueMine))
                {
                    notificationSetting.NextCheck = nextCheck;
                }

                await _db.SaveChangesAsync();
            }
            else
            {
                _logger.LogInformation($"Ignoring {typeName} notification for user {participant.UserId}, participant {participant.Id}, activity {participant.ActivityId}");
            }

            return Result.Success();
        }

        #endregion Private
    }
}
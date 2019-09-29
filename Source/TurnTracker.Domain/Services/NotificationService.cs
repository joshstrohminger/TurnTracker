using System;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly TurnContext _db;
        private readonly ILogger<NotificationService> _logger;

        #endregion

        #region ctor

        public NotificationService(TurnContext db, ILogger<NotificationService> logger)
        {
            _db = db;
            _logger = logger;
        }

        #endregion

        #region Public

        public Result UpdateNotificationSetting(int participantId, NotificationType type, bool sms, bool email, bool push)
        {
            try
            {
                var setting = _db.NotificationSettings.SingleOrDefault(x => x.ParticipantId == participantId && x.Type == type);
                if (setting is null)
                {
                    // only add a setting if one of the notification types is selected
                    if (sms || email || push)
                    {
                        setting = new NotificationSetting
                        {
                            ParticipantId = participantId,
                            Type = type,
                            Sms = sms,
                            Email = email,
                            Push = push
                        };
                        _db.NotificationSettings.Add(setting);
                    }
                }
                else
                {
                    if (sms || email || push)
                    {
                        setting.Sms = sms;
                        setting.Email = email;
                        setting.Push = push;
                        _db.NotificationSettings.Update(setting);
                    }
                    else
                    {
                        // delete settings that don't have any notification type selected
                        _db.NotificationSettings.Remove(setting);
                    }
                }

                _db.SaveChanges();
                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update notification settings");
                return Result.Fail(e.Message);
            }
        }

        #endregion
    }
}
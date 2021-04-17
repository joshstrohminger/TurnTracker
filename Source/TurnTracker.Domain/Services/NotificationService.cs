using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Services
{
    public class NotificationService : INotificationService
    {
        #region Fields

        private readonly TurnContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationService> _logger;

        #endregion

        #region ctor

        public NotificationService(TurnContext db, ILogger<NotificationService> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        #endregion

        #region Public

        public async Task<Result> UpdateDismissTimeOfDayAsync(int participantId, TimeSpan time)
        {
            try
            {
                var participant = await _db.Participants.FindAsync(participantId);
                if(participant != null)
                {
                    participant.DismissUntilTimeOfDay = time;
                    await _db.SaveChangesAsync();
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to update dismiss time of day to {time} for participant {participantId}");
                return Result.Failure(e.Message);
            }
        }

        public Result UpdateNotificationSetting(NotificationInfo info)
        {
            try
            {
                var setting = _db.NotificationSettings
                    .SingleOrDefault(x => x.ParticipantId == info.ParticipantId && x.Type == info.Type);
                var takeTurns = _db.Participants.Include(x => x.Activity)
                    .Select(x => new {x.Id, x.Activity.TakeTurns})
                    .Single(x => x.Id == info.ParticipantId)
                    .TakeTurns;
                if (setting is null)
                {
                    // only add a setting if one of the notification types is selected
                    if (info.AnyActive && info.Type.IsAllowed(takeTurns))
                    {
                        setting = _mapper.Map<NotificationSetting>(info);
                        setting.Origin = NotificationOrigin.Participant;
                        _db.NotificationSettings.Add(setting);
                    }
                }
                else
                {
                    if (info.AnyActive && info.Type.IsAllowed(takeTurns))
                    {
                        _mapper.Map(info, setting);
                        setting.Origin = NotificationOrigin.Participant;
                        _db.NotificationSettings.Update(setting);
                    }
                    else
                    {
                        // delete settings that don't have any notification type selected
                        _db.NotificationSettings.Remove(setting);
                    }
                }

                _db.SaveChanges();
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to update notification settings");
                return Result.Failure(e.Message);
            }
        }

        #endregion
    }
}
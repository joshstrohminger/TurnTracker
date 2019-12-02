using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TurnService : ITurnService
    {
        private readonly TurnContext _db;
        private readonly ILogger<TurnService> _logger;
        private readonly IMapper _mapper;

        public TurnService(TurnContext db, ILogger<TurnService> logger, IMapper mapper)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
        }

        public Result EnsureSeedActivities()
        {
            try
            {
                if (!_db.Activities.Any())
                {
                    var userIds = _db.Users.Select(x => x.Id).ToArray();

                    // first activity
                    var activityIdResult = AddActivity(userIds[0], "Clean Litterbox", true, 2, Unit.Day);
                    if (activityIdResult.IsFailure)
                    {
                        return activityIdResult;
                    }

                    var activityId = activityIdResult.Value;
                    var participantResult = AddParticipants(activityId, userIds);
                    if (participantResult.IsFailure)
                    {
                        return participantResult;
                    }

                    for(var i = 1; i < 8; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // second activity
                    activityIdResult = AddActivity(userIds[1], "Flea Meds", true, 1, Unit.Month);
                    if (activityIdResult.IsFailure)
                    {
                        return activityIdResult;
                    }

                    activityId = activityIdResult.Value;
                    participantResult = AddParticipants(activityId, userIds);
                    if (participantResult.IsFailure)
                    {
                        return participantResult;
                    }

                    for (var i = 1; i < 25; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // third activity
                    activityIdResult = AddActivity(userIds[0], "Take Out the Trash", false);
                    if (activityIdResult.IsFailure)
                    {
                        return activityIdResult;
                    }

                    activityId = activityIdResult.Value;
                    participantResult = AddParticipants(activityId, userIds);
                    if (participantResult.IsFailure)
                    {
                        return participantResult;
                    }

                    for (var i = 1; i < 37; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // fourth activity, disabled
                    activityIdResult = AddActivity(userIds[1], "Do the Dishes", true);
                    if (activityIdResult.IsFailure)
                    {
                        return activityIdResult;
                    }

                    activityId = activityIdResult.Value;
                    participantResult = AddParticipants(activityId, userIds);
                    if (participantResult.IsFailure)
                    {
                        return participantResult;
                    }

                    for (var i = 1; i < 19; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    return SetActivityDisabled(activityId, true);
                }
                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to seed activities");
                return Result.Failure(e.Message);
            }
        }

        public IEnumerable<Activity> GetAllActivities()
        {
            return _db.Activities.AsNoTracking();
        }

        public IEnumerable<Activity> GetActivitiesByOwner(int userId)
        {
            return _db.Activities
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);
        }

        public EditableActivity GetActivityForEdit(int id)
        {
            var activity = _db.Activities
                .AsNoTracking()
                .Include(x => x.Participants)
                .ThenInclude(x => x.User)
                .SingleOrDefault(x => x.Id == id);

            return activity is null ? null : _mapper.Map<EditableActivity>(activity);
        }

        public Result<Activity> SaveActivity(EditableActivity activity)
        {
            throw new NotImplementedException();
        }

        public Turn GetTurn(int id)
        {
            return _db.Turns
                .AsNoTracking()
                .Include(x => x.Creator)
                .Include(x => x.Modifier)
                .Include(x => x.User)
                .SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<Activity> GetActivitiesByParticipant(int userId)
        {
            return _db.Participants
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.Activity.CurrentTurnUser)
                .Select(x => x.Activity)
                .OrderBy(x => x.IsDisabled)
                .ThenByDescending(x => x.Due)
                .ThenBy(x => x.Name);
        }

        private Activity GetActivity(int activityId, bool readOnly, bool includeTurns)
        {
            IQueryable<Activity> query = _db.Activities;

            if (readOnly)
            {
                query = query.AsNoTracking();
            }

            if (includeTurns)
            {
                query = query.Include(x => x.Turns);
            }

            return query
                .Include(x => x.Owner)
                .Include(x => x.Participants).ThenInclude(x => x.User)
                .Include(x => x.Participants).ThenInclude(x => x.NotificationSettings)
                .SingleOrDefault(x => x.Id == activityId);
        }

        public ActivityDetails GetActivityDetailsShallow(int activityId, int userId)
        {
            var activity = GetActivity(activityId, true, false);
            return activity is null ? null : ActivityDetails.Populate(activity, userId);
        }

        public ActivityDetails GetActivityDetails(int activityId, int userId)
        {
            var activity = GetActivity(activityId, true, true);
            return activity is null ? null : ActivityDetails.Calculate(activity, userId);
        }

        public Result<ActivityDetails> TakeTurn(int activityId, int byUserId, int forUserId, DateTimeOffset when)
        {
            try
            {
                _db.Turns.Add(new Turn
                {
                    ActivityId = activityId,
                    CreatorId = byUserId,
                    UserId = forUserId,
                    Occurred = when
                });

                var activity = GetActivity(activityId, false, true);
                if (activity == null)
                {
                    return Result.Failure<ActivityDetails>("no such activity");
                }

                var details = ActivityDetails.Calculate(activity, byUserId);

                _db.SaveChanges();

                return Result.Ok(details);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to take turn");
                return Result.Failure<ActivityDetails>(e.Message);
            }
        }

        public Result<ActivityDetails> SetTurnDisabled(int turnId, int byUserId, bool disabled)
        {
            var turn = _db.Turns.Find(turnId);
            if (turn != null)
            {
                if (turn.IsDisabled != disabled)
                {
                    turn.IsDisabled = disabled;
                    turn.ModifierId = byUserId;
                    _db.SaveChanges();
                }

                return Result.Ok(GetActivityDetails(turn.ActivityId, byUserId));
            }

            return Result.Failure<ActivityDetails>("Invalid turn id");
        }

        public Result SetActivityDisabled(int activityId, bool disabled)
        {
            var activity = _db.Activities.Find(activityId);
            if (activity is null) return Result.Failure("Invalid activity");

            activity.IsDisabled = disabled;
            _db.SaveChanges();

            return Result.Ok();
        }

        public Result<int> AddActivity(int ownerId, string name, bool takeTurns, uint? periodCount = null, Unit? periodUnit = null)
        {
            try
            {
                TimeSpan? period = null;
                if (!periodUnit.HasValue)
                {
                    periodCount = null;
                }
                else if (!periodCount.HasValue)
                {
                    periodUnit = null;
                }
                else if (periodCount.Value == 0)
                {
                    return Result.Failure<int>("invalid period count");
                }
                else
                {
                    switch (periodUnit.Value)
                    {
                        case Unit.Hour:
                            period = TimeSpan.FromHours(periodCount.Value);
                            break;
                        case Unit.Day:
                            period = TimeSpan.FromDays(periodCount.Value);
                            break;
                        case Unit.Week:
                            period = TimeSpan.FromDays(7 * periodCount.Value);
                            break;
                        case Unit.Month:
                            period = TimeSpan.FromDays(365.25 / 12);
                            break;
                        case Unit.Year:
                            period = TimeSpan.FromDays(365.25);
                            break;
                        default:
                            return Result.Failure<int>("invalid period unit");
                    }
                }

                var activity = new Activity
                {
                    OwnerId = ownerId,
                    Name = name,
                    PeriodCount = periodCount,
                    PeriodUnit = periodUnit,
                    Period = period,
                    TakeTurns = takeTurns
                };
                _db.Activities.Add(activity);
                _db.SaveChanges();
                return Result.Ok(activity.Id);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add activity");
                return Result.Failure<int>(e.Message);
            }
        }

        public Result AddParticipants(int activityId, params int[] userIds)
        {
            if (userIds is null || userIds.Length == 0)
            {
                return Result.Failure("missing user ids");
            }

            try
            {
                foreach (var userId in userIds)
                {
                    _db.Participants.Add(new Participant
                    {
                        ActivityId = activityId,
                        UserId = userId
                    });
                }

                _db.SaveChanges();
                return Result.Ok();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to add users {string.Join(',', userIds)} to activity {activityId}");
                return Result.Failure(e.Message);
            }
        }
    }
}
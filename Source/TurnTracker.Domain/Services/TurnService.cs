using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TurnTracker.Common;
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
                    var userIds = _db.Users.Take(2).Select(x => x.Id).ToArray();

                    // first activity
                    var activityResult = SaveActivity(new EditableActivity { Name = "Clean Litterbox", TakeTurns = true, PeriodCount = 2, PeriodUnit = Unit.Day, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList() }, userIds[0]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult;
                    }
                    var activityId = activityResult.Value;
                    for(var i = 1; i < 8; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // second activity
                    activityResult = SaveActivity(new EditableActivity { Name = "Flea Meds", TakeTurns = true, PeriodCount = 1, PeriodUnit = Unit.Month, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList() }, userIds[1]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult;
                    }
                    activityId = activityResult.Value;
                    for (var i = 1; i < 25; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // third activity
                    activityResult = SaveActivity(new EditableActivity { Name = "Take Out the Trash", TakeTurns = false, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList()}, userIds[0]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult;
                    }
                    activityId = activityResult.Value;
                    for (var i = 1; i < 37; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        TakeTurn(activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // fourth activity, disabled
                    activityResult = SaveActivity(new EditableActivity{Name = "Do the Dishes", TakeTurns = true, Participants = userIds.Select(id => new UserInfo{Id = id}).ToList()}, userIds[1]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult;
                    }
                    activityId = activityResult.Value;
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

        public Result<int, ValidityError> SaveActivity(EditableActivity activity, int ownerId)
        {
            try
            {
                if (activity is null)
                {
                    return ValidityError.ForInvalidObject<int>("no activity provided");
                }

                if (string.IsNullOrWhiteSpace(activity.Name))
                {
                    return ValidityError.ForInvalidObject<int>("empty name");
                }

                TimeSpan? period = null;
                if (!activity.PeriodUnit.HasValue)
                {
                    activity.PeriodCount = null;
                }
                else if (!activity.PeriodCount.HasValue)
                {
                    activity.PeriodUnit = null;
                }
                else if (activity.PeriodCount.Value == 0)
                {
                    return ValidityError.ForInvalidObject<int>("invalid period count");
                }
                else
                {
                    switch (activity.PeriodUnit.Value)
                    {
                        case Unit.Hour:
                            period = TimeSpan.FromHours(activity.PeriodCount.Value);
                            break;
                        case Unit.Day:
                            period = TimeSpan.FromDays(activity.PeriodCount.Value);
                            break;
                        case Unit.Week:
                            period = TimeSpan.FromDays(7 * activity.PeriodCount.Value);
                            break;
                        case Unit.Month:
                            period = TimeSpan.FromDays(365.25 / 12);
                            break;
                        case Unit.Year:
                            period = TimeSpan.FromDays(365.25);
                            break;
                        default:
                            return ValidityError.ForInvalidObject<int>("invalid period unit");
                    }
                }

                activity.Participants ??= new List<UserInfo>();

                if (activity.Id < 0)
                {
                    return ValidityError.ForInvalidObject<int>("invalid ID");
                }

                var add = false;
                Activity activityToUpdate;
                if (activity.Id == 0)
                {
                    add = true;
                    activityToUpdate = new Activity();
                }
                else
                {
                    activityToUpdate = GetActivity(activity.Id, false, false);
                    if (activityToUpdate is null)
                    {
                        return ValidityError.ForInvalidObject<int>("invalid ID");
                    }

                    if (activityToUpdate.IsDisabled)
                    {
                        return ValidityError.ForInvalidObject<int>("activity is disabled");
                    }
                }

                activityToUpdate.OwnerId = ownerId;
                activityToUpdate.Name = activity.Name;
                activityToUpdate.PeriodCount = activity.PeriodCount;
                activityToUpdate.PeriodUnit = activity.PeriodUnit;
                activityToUpdate.Period = period;
                activityToUpdate.TakeTurns = activity.TakeTurns;
                activityToUpdate.Participants = activity.Participants.Select(p => p.Id).Append(ownerId).Distinct()
                    .Select(id => new Participant {UserId = id}).ToList();

                if (add)
                {
                    _db.Activities.Add(activityToUpdate);
                }
                else
                {
                    _db.Activities.Update(activityToUpdate);
                }

                _db.SaveChanges();
                
                return Result.Ok<int, ValidityError>(activityToUpdate.Id);
            }
            catch (Exception e)
            {
                var message = $"Failed to save activity '{activity?.Id}'";
                _logger.LogError(e, message);
                return ValidityError.ForInvalidObject<int>(message);
            }
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
    }
}
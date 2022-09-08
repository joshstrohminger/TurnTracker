using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Configuration;
using TurnTracker.Domain.Interfaces;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Services
{

    public class TurnService : ITurnService
    {
        private readonly TurnContext _db;
        private readonly ILogger<TurnService> _logger;
        private readonly IMapper _mapper;
        private readonly IPushNotificationService _pushNotificationService;
        private readonly IOptions<AppSettings> _appSettings;

        public TurnService(TurnContext db, ILogger<TurnService> logger, IMapper mapper, IPushNotificationService pushNotificationService, IOptions<AppSettings> appSettings)
        {
            _db = db;
            _logger = logger;
            _mapper = mapper;
            _pushNotificationService = pushNotificationService;
            _appSettings = appSettings;
        }

        public Result CreateMissingParticipants()
        {
            try
            {
                return Result.Combine(_db.Activities.Include(a => a.Participants)
                    .Where(a => a.Participants.Count == 0).Select(a => new { a.Id, a.OwnerId }).ToList().Select(info =>
                    {
                        _logger.LogWarning($"Missing participants for activity {info.Id}");
                        var activity = GetActivityForEdit(info.Id);
                        if (activity.IsDisabled)
                        {
                            _logger.LogError($"Can't add missing participants to disabled activity {info.Id} - {activity.Name}");
                            return Result.Success();
                        }
                        var result = SaveActivity(activity, info.OwnerId);
                        return Result.SuccessIf(result.IsSuccess, result.IsSuccess ? "" : result.Error.Message);
                    }));
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to create missing participants");
                return Result.Failure(e.Message);
            }
        }

        public async Task<Result> EnsureSeedActivitiesAsync()
        {
            try
            {
                _logger.LogInformation("Seeding");
                if (!_db.Activities.Any())
                {
                    var userIds = _db.Users.OrderBy(x => x.Id).Take(2).Select(x => x.Id).ToArray();

                    // first activity
                    var activityResult = SaveActivity(new EditableActivity { Name = "Clean Litterbox", TakeTurns = true, PeriodCount = 2, PeriodUnit = Unit.Day, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList() }, userIds[0]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult.MapError(e => e.ToString());
                    }
                    var activityId = activityResult.Value.Id;
                    var activity = GetActivityDetailsShallow(activityId, userIds[0]);
                    for(var i = 1; i < 8; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        await TakeTurnAsync(activity.ModifiedDate, activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // second activity
                    activityResult = SaveActivity(new EditableActivity { Name = "Flea Meds", TakeTurns = true, PeriodCount = 1, PeriodUnit = Unit.Month, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList() }, userIds[1]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult.MapError(e => e.ToString());
                    }
                    activityId = activityResult.Value.Id;
                    activity = GetActivityDetailsShallow(activityId, userIds[0]);
                    for (var i = 1; i < 25; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        await TakeTurnAsync(activity.ModifiedDate, activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // third activity
                    activityResult = SaveActivity(new EditableActivity { Name = "Take Out the Trash", TakeTurns = false, Participants = userIds.Select(id => new UserInfo { Id = id }).ToList()}, userIds[0]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult.MapError(e => e.ToString());
                    }
                    activityId = activityResult.Value.Id;
                    activity = GetActivityDetailsShallow(activityId, userIds[0]);
                    for (var i = 1; i < 37; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        await TakeTurnAsync(activity.ModifiedDate, activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    // fourth activity, disabled
                    activityResult = SaveActivity(new EditableActivity{Name = "Do the Dishes", TakeTurns = true, Participants = userIds.Select(id => new UserInfo{Id = id}).ToList()}, userIds[1]);
                    if (activityResult.IsFailure)
                    {
                        return activityResult.MapError(e => e.ToString());
                    }
                    activityId = activityResult.Value.Id;
                    activity = GetActivityDetailsShallow(activityId, userIds[0]);
                    for (var i = 1; i < 19; i++)
                    {
                        var forUser = userIds[i % 2];
                        var byUser = userIds[i / 2 % 2];
                        await TakeTurnAsync(activity.ModifiedDate, activityId, byUser, forUser, DateTimeOffset.Now.Subtract(TimeSpan.FromDays(i)));
                    }

                    return SetActivityDisabled(activityId, userIds[1], true);
                }
                return Result.Success();
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
                .Include(x => x.DefaultNotificationSettings)
                .Include(x => x.Participants)
                .ThenInclude(x => x.User)
                .AsSingleQuery()
                .SingleOrDefault(x => x.Id == id);

            return activity is null ? null : _mapper.Map<EditableActivity>(activity);
        }

        public Result<ActivityDetails, ValidityError> SaveActivity(EditableActivity activity, int ownerId)
        {
            try
            {
                if (activity is null)
                {
                    return ValidityError.ForInvalidObject<ActivityDetails>("no activity provided");
                }

                if (string.IsNullOrWhiteSpace(activity.Name))
                {
                    return ValidityError.ForInvalidObject<ActivityDetails>("empty name");
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
                    return ValidityError.ForInvalidObject<ActivityDetails>("invalid period count");
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
                            period = TimeSpan.FromDays(365.25 / 12 * activity.PeriodCount.Value);
                            break;
                        case Unit.Year:
                            period = TimeSpan.FromDays(365.25 * activity.PeriodCount.Value);
                            break;
                        default:
                            return ValidityError.ForInvalidObject<ActivityDetails>("invalid period unit");
                    }
                }

                activity.Participants ??= new List<UserInfo>();

                // sanitize the default notification settings to ensure there's only one per type
                var defaultNotificationSettings =
                    (activity.DefaultNotificationSettings ?? new List<NotificationInfo>())
                    .Where(x => x.AnyActive && x.Type.IsAllowed(activity.TakeTurns))
                    .GroupBy(x => x.Type)
                    .Select(g => g.First())
                    .ToDictionary(x => x.Type);

                if (activity.Id < 0)
                {
                    return ValidityError.ForInvalidObject<ActivityDetails>("invalid ID");
                }

                var userIds = activity.Participants.Select(p => p.Id).Append(ownerId).ToHashSet();

                var add = false;
                Activity activityToUpdate;
                if (activity.Id == 0)
                {
                    add = true;
                    activityToUpdate = new Activity
                    {
                        Participants = _db.Users.Where(user => userIds.Contains(user.Id)).ToList().Select(CreateNewParticipant).ToList(),
                        DefaultNotificationSettings = _mapper.Map<List<DefaultNotificationSetting>>(defaultNotificationSettings.Values),
                        Owner = _db.Users.Find(ownerId),
                        Turns = new List<Turn>(0)
                    };
                    foreach (var participant in activityToUpdate.Participants)
                    {
                        participant.NotificationSettings.AddRange(_mapper.Map<List<NotificationSetting>>(defaultNotificationSettings.Values));
                    }
                }
                else
                {
                    activityToUpdate = GetActivity(activity.Id, false, true, true);
                    if (activityToUpdate is null)
                    {
                        return ValidityError.ForInvalidObject<ActivityDetails>("invalid ID");
                    }

                    if (activityToUpdate.IsDisabled)
                    {
                        return ValidityError.ForInvalidObject<ActivityDetails>("activity is disabled");
                    }

                    // remove any participants that should no longer be there
                    activityToUpdate.Participants.RemoveAll(x => !userIds.Contains(x.UserId));

                    // remove any existing participants from the list so we're left with only new participants
                    userIds.ExceptWith(activityToUpdate.Participants.Select(x => x.UserId));

                    // add new participants
                    activityToUpdate.Participants.AddRange(_db.Users.Where(user => userIds.Contains(user.Id)).ToList().Select(CreateNewParticipant));

                    // remove any default notifications that should no longer be there
                    activityToUpdate.DefaultNotificationSettings.RemoveAll(x =>
                    {
                        var remove = !defaultNotificationSettings.ContainsKey(x.Type);

                        if (remove)
                        {
                            foreach (var participant in activityToUpdate.Participants)
                            {
                                participant.NotificationSettings.RemoveAll(y => y.Type == x.Type && y.Origin == NotificationOrigin.Default);
                            }
                        }

                        return remove;
                    });

                    // remove any participant notifications that are no longer valid because the activity no longer has turns needed
                    if (activityToUpdate.TakeTurns && !activity.TakeTurns)
                    {
                        foreach (var participant in activityToUpdate.Participants)
                        {
                            participant.NotificationSettings.RemoveAll(x => !x.Type.IsAllowed(activity.TakeTurns));
                        }
                    }

                    // update any existing default notification and remove it from the new list
                    foreach(var noteToUpdate in activityToUpdate.DefaultNotificationSettings)
                    {
                        if(defaultNotificationSettings.Remove(noteToUpdate.Type, out var updatedNote))
                        {
                            _mapper.Map(updatedNote, noteToUpdate);

                            // update any existing participant notifications that originated from a default notification
                            foreach(var participantNote in activityToUpdate.Participants.SelectMany(x => x.NotificationSettings).Where(x => x.Type == updatedNote.Type && x.Origin == NotificationOrigin.Default))
                            {
                                //make sure we don't clear the participant ID
                                updatedNote.ParticipantId = participantNote.ParticipantId;
                                _mapper.Map(updatedNote, participantNote);
                            }
                        }
                    }

                    // add any remaining notifications
                    foreach (var note in defaultNotificationSettings.Values)
                    {
                        activityToUpdate.DefaultNotificationSettings.Add(_mapper.Map<DefaultNotificationSetting>(note));
                        foreach (var participant in activityToUpdate.Participants.Where(participant =>
                            participant.NotificationSettings.All(x => x.Type != note.Type)))
                        {
                            participant.NotificationSettings.Add(_mapper.Map<NotificationSetting>(note));
                        }
                    }
                }

                activityToUpdate.OwnerId = ownerId;
                activityToUpdate.Name = activity.Name;
                activityToUpdate.Description = string.IsNullOrWhiteSpace(activity.Description)
                    ? null
                    : activity.Description.Trim();
                activityToUpdate.PeriodCount = activity.PeriodCount;
                activityToUpdate.PeriodUnit = activity.PeriodUnit;
                activityToUpdate.Period = period;
                activityToUpdate.TakeTurns = activity.TakeTurns;

                if (add)
                {
                    _db.Activities.Add(activityToUpdate);
                }
                else
                {
                    _db.Activities.Update(activityToUpdate);
                }

                var details = ActivityDetails.Calculate(activityToUpdate, ownerId, _mapper);

                _db.SaveChanges();
                details.Update(activityToUpdate);
                return Result.Success<ActivityDetails, ValidityError>(details);
            }
            catch (Exception e)
            {
                var message = $"Failed to save activity '{activity?.Id}'";
                _logger.LogError(e, message);
                return ValidityError.ForInvalidObject<ActivityDetails>(message);
            }

            Participant CreateNewParticipant(User user)
            {
                return new Participant
                {
                    UserId = user.Id,
                    User = user,
                    DismissUntilTimeOfDay = _appSettings.Value.PushNotifications.DefaultDismissTime,
                    NotificationSettings = new List<NotificationSetting>()
                };
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

        private Activity GetActivity(int activityId, bool readOnly, bool includeTurns, bool includeDefaultNotifications = false)
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

            if (includeDefaultNotifications)
            {
                query = query.Include(x => x.DefaultNotificationSettings);
            }

            return query
                .Include(x => x.Owner)
                .Include(x => x.Participants).ThenInclude(x => x.User)
                .Include(x => x.Participants).ThenInclude(x => x.NotificationSettings)
                .AsSingleQuery()
                .SingleOrDefault(x => x.Id == activityId);
        }

        public ActivityDetails GetActivityDetailsShallow(int activityId, int userId)
        {
            var activity = GetActivity(activityId, true, false);
            return activity is null ? null : ActivityDetails.Populate(activity, userId, _mapper);
        }

        public ActivityDetails GetActivityDetails(int activityId, int userId)
        {
            var activity = GetActivity(activityId, true, true);
            return activity is null ? null : ActivityDetails.Calculate(activity, userId, _mapper);
        }

        public async Task<Result<ActivityDetails, TurnError>> TakeTurnAsync(DateTimeOffset activityModifiedDate, int activityId, int byUserId, int forUserId, DateTimeOffset when)
        {
            try
            {
                var now = DateTimeOffset.Now;

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
                    _logger.LogError($"Activity {activityId} is missing");
                    return Result.Failure<ActivityDetails,TurnError>(TurnError.ActivityMissing);
                }

                if(_appSettings.Value.ValidateActivityModifiedDate && activity.ModifiedDate != activityModifiedDate)
                {
                    _logger.LogWarning($"Activity {activity.Id} was modified {activity.ModifiedDate} and doesn't match {activityModifiedDate}");
                    return Result.Failure<ActivityDetails, TurnError>(TurnError.ActivityModified);
                }

                var details = ActivityDetails.Calculate(activity, byUserId, _mapper);
                
                var turnTaker = await _db.Users.FindAsync(forUserId);
                FormattableString fs = $"{turnTaker.DisplayName} took a turn.";
                var myTurnBuilder = new StringBuilder().AppendFormattable(fs);
                var otherTurnBuilder = new StringBuilder().AppendFormattable(fs);
                if (details.CurrentTurnUserId.HasValue)
                {
                    otherTurnBuilder.AppendFormattable($" It's {details.CurrentTurnUserDisplayName}'s turn.");
                    myTurnBuilder.AppendFormattable($" It's your turn.");
                }
                if (details.Due.HasValue)
                {
                    fs = $" Due in {(details.Due.Value - now).ToDisplayString()}.";
                    otherTurnBuilder.AppendFormattable(fs);
                    myTurnBuilder.AppendFormattable(fs);
                }

                var myTurnMessage = myTurnBuilder.ToString();
                var otherTurnMessage = otherTurnBuilder.ToString();
                var url = $"/activity/{activityId}";

                var failures = new List<PushFailure>();

                foreach (var participant in activity.Participants)
                {
                    var pushNotified = false;

                    foreach (var setting in participant.NotificationSettings.OrderBy(x => x.Type))
                    {
                        switch (setting.Type)
                        {
                            case NotificationType.OverdueAnybody:
                            case NotificationType.OverdueMine:
                                setting.NextCheck = now;

                                // send a close push notification in case they still have a previous notification open but not if they
                                // already got a notification about a turn being taken because that will replace any existing notification
                                if (setting.Push && !pushNotified)
                                {
                                    failures.AddRange(await _pushNotificationService.SendCloseToAllDevicesAsync("turn", setting.Participant.UserId, activityId.ToString()));
                                    pushNotified = true;
                                }
                                break;
                            case NotificationType.TurnTakenAnybody:
                                if(setting.Push)
                                {
                                    failures.AddRange(await _pushNotificationService.SendToAllDevicesAsync("turn", setting.Participant.UserId,
                                        activity.Name, otherTurnMessage, url, activityId.ToString()));
                                    pushNotified = true;
                                }
                                break;
                            case NotificationType.TurnTakenMine:
                                if (details.CurrentTurnUserId.HasValue && setting.Push && setting.Participant.UserId == details.CurrentTurnUserId)
                                {
                                    failures.AddRange(await _pushNotificationService.SendToAllDevicesAsync("turn", setting.Participant.UserId,
                                        activity.Name, myTurnMessage, url, activityId.ToString()));
                                    pushNotified = true;
                                }
                                break;
                            default:
                                _logger.LogError($"Unhandled notification type {setting.Type}");
                                break;
                        }
                    }
                }

                // Ensure we are done sending each push message before cleaning up failures and continuing
                await _pushNotificationService.CleanupFailuresAsync(failures);

                // Always mark the activity as modified when taking a turn so our checks elsewhere that compare
                // the modified timestamp will still work for activities that wouldn't normally have anything update,
                // like when they're non-periodic or the next-turn user doesn't change.
                _db.Entry(activity).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                details.Update(activity);

                return Result.Success<ActivityDetails,TurnError>(details);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to take a turn");
                return Result.Failure<ActivityDetails,TurnError>(TurnError.Exception);
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

                    var activity = GetActivity(turn.ActivityId, false, true);
                    if (activity == null)
                    {
                        return Result.Failure<ActivityDetails>("no such activity");
                    }
                    var details = ActivityDetails.Calculate(activity, byUserId, _mapper);

                    _db.Entry(activity).State = EntityState.Modified;
                    _db.SaveChanges();
                    details.Update(activity);
                    return Result.Success(details);
                }

                return Result.Success(GetActivityDetails(turn.ActivityId, byUserId));
            }

            return Result.Failure<ActivityDetails>("Invalid turn id");
        }

        public Result<ActivityDetails> SetActivityDisabled(int activityId, int byUserId, bool disabled)
        {
            var activity = _db.Activities.Find(activityId);
            if (activity is null) return Result.Failure<ActivityDetails>("Invalid activity");

            activity.IsDisabled = disabled;
            _db.SaveChanges();

            return Result.Success(GetActivityDetails(activityId, byUserId));
        }

        public Result DeleteActivity(int activityId)
        {
            var activity = _db.Activities.Find(activityId);
            if (activity is null) return Result.Failure<ActivityDetails>("Invalid activity");

            _db.Remove(activity);
            _db.SaveChanges();

            return Result.Success();
        }
    }
}
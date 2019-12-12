using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TurnTracker.Common;
using TurnTracker.Data;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        #region Fields

        private readonly TurnContext _db;

        #endregion

        #region ctor

        public ResourceAuthorizationService(TurnContext db)
        {
            _db = db;
        }

        #endregion

        public bool IsParticipantOf(int activityId, int userId)
        {
            return _db.Participants.AsNoTracking()
                .Any(x => x.ActivityId == activityId && x.UserId == userId);
        }

        public Result CanTakeTurn(int activityId, params int[] userIds)
        {
            if (userIds is null || userIds.Length == 0)
            {
                return Result.Failure("Invalid user IDs");
            }

            var activity = _db.Activities.AsNoTracking()
                .Include(x => x.Participants)
                .SingleOrDefault(x => x.Id == activityId);

            if (activity is null)
            {
                return Result.Failure("Invalid activity");
            }

            if (activity.IsDisabled)
            {
                return Result.Failure("Activity is disabled");
            }

            if (!activity.Participants.Select(x => x.UserId).ContainsSubset(userIds))
            {
                return Result.Failure("Not a participant of this activity");
            }

            return Result.Ok();
        }

        public bool IsOwnerOf(int activityId, int userId)
        {
            return _db.Activities.AsNoTracking()
                       .Any(x => x.Id == activityId && x.OwnerId == userId);
        }

        public bool CanModifyTurn(int turnId, int userId)
        {
            var turn = _db.Turns.AsNoTracking()
                .Include(x => x.Activity)
                .SingleOrDefault(x => x.Id == turnId);
            return turn != null && !turn.Activity.IsDisabled && (turn.UserId == userId || turn.CreatorId == userId);
        }

        public bool CanModifyActivity(int activityId, int userId)
        {
            return _db.Activities.AsNoTracking()
                .Any(x => x.Id == activityId && x.OwnerId == userId);
        }

        public bool CanModifyParticipant(int participantId, int userId)
        {
            return _db.Participants.AsNoTracking()
                       .SingleOrDefault(x => x.Id == participantId && !x.Activity.IsDisabled)?.UserId == userId;
        }
    }
}
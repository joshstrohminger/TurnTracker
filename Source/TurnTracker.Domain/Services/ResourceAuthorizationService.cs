using System;
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

        public bool AreParticipantsOf(int activityId, params int[] userIds)
        {
            return userIds?.Length > 0 && _db.Participants.AsNoTracking().Where(x => x.ActivityId == activityId)
                       .Select(x => x.UserId)
                       .ContainsSubset(userIds);
        }

        public bool IsOwnerOf(int activityId, int userId)
        {
            return _db.Activities.AsNoTracking().SingleOrDefault(x => x.Id == activityId)?.OwnerId == userId;
        }

        public bool CanModifyTurn(int turnId, int userId)
        {
            var turn = _db.Turns.AsNoTracking().SingleOrDefault(x => x.Id == turnId);
            return turn?.UserId == userId || turn?.CreatorId == userId;
        }

        public bool CanModifyParticipant(int participantId, int userId)
        {
            return _db.Participants.AsNoTracking().SingleOrDefault(x => x.Id == participantId)?.UserId == userId;
        }

        public static Result<TResult> AsResult<TResult>(Func<TResult> func)
        {
            try
            {
                return Result.Ok(func());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Result.Fail<TResult>(e.Message);
            }
        }
    }
}
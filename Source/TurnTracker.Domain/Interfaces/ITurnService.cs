using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public interface ITurnService
    {
        Result EnsureSeedActivities();
        IEnumerable<Activity> GetAllActivities();
        IEnumerable<Activity> GetActivitiesByOwner(int userId);
        IEnumerable<Activity> GetActivitiesByParticipant(int userId);
        Activity GetActivity(int activityId);
        ActivityInfo GetActivityInfo(int activityId);
        Result TakeTurn(int activityId, int byUserId, int forUserId, DateTimeOffset when);
        Result DisableTurn(int turnId, int byUserId);
        Result<int> AddActivity(int ownerId, string name, uint? periodCount = null, Unit? periodUnit = null);
        Result AddParticipants(int activityId, params int[] userIds);
    }
}
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
        ActivityDetails GetActivityDetails(int activityId, int userId);
        ActivityDetails GetActivityDetailsShallow(int activityId, int userId);
        Result<ActivityDetails> TakeTurn(int activityId, int byUserId, int forUserId, DateTimeOffset when);
        Result<ActivityDetails> SetTurnDisabled(int turnId, int byUserId, bool disabled);
        Result<int> AddActivity(int ownerId, string name, bool takeTurns, uint? periodCount = null, Unit? periodUnit = null);
        Result AddParticipants(int activityId, params int[] userIds);
        Turn GetTurn(int id);
        Result SetActivityDisabled(int activityId, bool disabled);
        EditableActivity GetActivityForEdit(int id);
        Result<Activity> SaveActivity(EditableActivity activity);
    }
}
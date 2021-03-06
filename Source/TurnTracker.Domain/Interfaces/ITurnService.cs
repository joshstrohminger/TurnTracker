﻿using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using TurnTracker.Common;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public enum TurnError
    {
        Unknown,
        Exception,
        ActivityMissing,
        ActivityModified
    }

    public interface ITurnService
    {
        Result EnsureSeedActivities();
        IEnumerable<Activity> GetAllActivities();
        IEnumerable<Activity> GetActivitiesByOwner(int userId);
        IEnumerable<Activity> GetActivitiesByParticipant(int userId);
        ActivityDetails GetActivityDetails(int activityId, int userId);
        ActivityDetails GetActivityDetailsShallow(int activityId, int userId);
        Result<ActivityDetails, TurnError> TakeTurn(DateTimeOffset activityModifiedDate, int activityId, int byUserId, int forUserId, DateTimeOffset when);
        Result<ActivityDetails> SetTurnDisabled(int turnId, int byUserId, bool disabled);
        Turn GetTurn(int id);
        Result<ActivityDetails> SetActivityDisabled(int activityId, int byUserId, bool disabled);
        EditableActivity GetActivityForEdit(int id);
        Result<int, ValidityError> SaveActivity(EditableActivity activity, int ownerId);
        Result DeleteActivity(int activityId);
    }
}
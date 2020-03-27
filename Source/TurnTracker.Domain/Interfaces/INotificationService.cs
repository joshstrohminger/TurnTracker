using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TurnTracker.Data.Entities;

namespace TurnTracker.Domain.Interfaces
{
    public interface INotificationService
    {
        Result UpdateNotificationSetting(int participantId, NotificationType type, bool sms, bool email, bool push);
        Task<Result> UpdateDismissTimeOfDayAsync(int participantId, TimeSpan time);
    }
}
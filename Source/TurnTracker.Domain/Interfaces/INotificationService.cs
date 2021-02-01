using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public interface INotificationService
    {
        Result UpdateNotificationSetting(NotificationInfo info);
        Task<Result> UpdateDismissTimeOfDayAsync(int participantId, TimeSpan time);
    }
}
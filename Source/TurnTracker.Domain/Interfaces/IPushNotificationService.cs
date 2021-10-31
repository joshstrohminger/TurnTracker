using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TurnTracker.Domain.Models;
using TurnTracker.Domain.Services;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushNotificationService
    {
        Task<Result> SendToOneDeviceAsync(int userId, string title, string message, string endpoint, string groupKey);
        Task<Result> SendCloseToOneDeviceAsync(int userId, string endpoint, string groupKey);
        Task<PushFailure[]> SendToAllDevicesAsync(string label, int userId, string title, string message, string url, string groupKey, params PushAction[] actions);
        Task<PushFailure[]> SendCloseToAllDevicesAsync(string label, int userId, string groupKey);
        Task CleanupFailuresAsync(IEnumerable<PushFailure> failures);
    }
}
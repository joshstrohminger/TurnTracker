using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using TurnTracker.Domain.Models;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushNotificationService
    {
        Task<Result> SendToOneDeviceAsync(int userId, string title, string message, string endpoint, string groupKey);
        Result SendToAllDevices(int userId, string title, string message, string url, string groupKey, params PushAction[] actions);
    }
}
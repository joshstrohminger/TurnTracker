using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushNotificationService
    {
        Result SaveSubscription(int userId, PushSubscription sub);
        Result RemoveSubscription(int userId, PushSubscription sub);
        Result SendToOneDevice(int userId, string message, string endpoint);
        Result SendToAllDevices(int userId, string message);
    }
}
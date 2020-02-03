using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushNotificationService
    {
        Task<Result> SendToOneDeviceAsync(int userId, string title, string message, string endpoint);
        Task<Result> SendToAllDevicesAsync(int userId, string title, string message);
    }
}
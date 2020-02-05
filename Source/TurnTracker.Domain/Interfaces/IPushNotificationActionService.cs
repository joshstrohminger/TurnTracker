using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushNotificationActionService
    {
        Task<Result> ActAsync(int userId, int participantId, string action);
    }
}
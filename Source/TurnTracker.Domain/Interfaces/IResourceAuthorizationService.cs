using CSharpFunctionalExtensions;

namespace TurnTracker.Domain.Interfaces
{
    public interface IResourceAuthorizationService
    {
        bool IsParticipantOf(int activityId, int userId);
        Result CanTakeTurn(int activityId, params int[] userIds);
        bool IsOwnerOf(int activityId, int userId);
        bool CanModifyTurn(int turnId, int userId);
        bool CanModifyActivity(int activityId, int userId);
        bool CanModifyParticipant(int participantId, int userId);
        bool CanDeleteSession(long loginId, int userId);
        bool CanDeleteDevice(int deviceAuthorizationId, int userId, long loginId);
    }
}
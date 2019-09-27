using CSharpFunctionalExtensions;

namespace TurnTracker.Domain.Interfaces
{
    public interface IResourceAuthorizationService
    {
        bool IsParticipantOf(int activityId, int userId);
        bool AreParticipantsOf(int activityId, params int[] userIds);
        bool IsOwnerOf(int activityId, int userId);
        bool CanModifyTurn(int turnId, int userId);
        bool CanModifyParticipant(int participantId, int userId);
    }
}
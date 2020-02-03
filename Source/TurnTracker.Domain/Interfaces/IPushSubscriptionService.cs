using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushSubscriptionService
    {
        Result SaveSubscription(int userId, PushSubscription sub);
        Result RemoveSubscription(int userId, PushSubscription sub);
        PushSubscription Get(int userId, string endpoint);
        IEnumerable<PushSubscription> Get(int userId);
    }
}
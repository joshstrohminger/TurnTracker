using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;

namespace TurnTracker.Domain.Interfaces
{
    public interface IPushSubscriptionService
    {
        Result SaveSubscription(int userId, PushSubscription sub);
        Task<Result> RemoveSubscriptionAsync(int userId, PushSubscription sub, bool save);
        PushSubscription Get(int userId, string endpoint);
        IEnumerable<PushSubscription> Get(int userId);
    }
}
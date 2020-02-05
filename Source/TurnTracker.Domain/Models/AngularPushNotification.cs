using System.Collections.Generic;
using Lib.Net.Http.WebPush;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TurnTracker.Domain.Models
{
    public class AngularPushNotification
    {
        public class NotificationAction
        {
            public string Action { get; }

            public string Title { get; }

            public NotificationAction(string action, string title)
            {
                Action = action;
                Title = title;
            }
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public string Title { get; set; }

        public string Body { get; set; }

        public string Icon { get; set; }

        public string Badge { get; set; }

        public bool Renotify { get; set; }

        public bool RequireInteraction { get; set; }

        public string Tag { get; set; }

        public IDictionary<string, object> Data { get; set; }

        public IList<NotificationAction> Actions { get; set; } = new List<NotificationAction>();

        public PushMessage ToPushMessage(string topic = null, int? timeToLive = null, PushMessageUrgency urgency = PushMessageUrgency.Normal)
        {
            return new PushMessage(JsonConvert.SerializeObject(new { notification = this }, JsonSerializerSettings))
            {
                Topic = topic,
                TimeToLive = timeToLive,
                Urgency = urgency
            };
        }
    }
}
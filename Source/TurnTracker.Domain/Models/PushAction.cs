﻿using System;

namespace TurnTracker.Domain.Models
{

    public class PushAction
    {
        public enum ActionType
        {
            Post
        }

        public string Title { get; }
        public string Action { get;  }
        public string Url { get; }
        public ActionType Type { get; }
        public string AdditionalData { get; }

        public PushAction(string action, string title, string url, string token) : this(action, title, ActionType.Post,
            url)
        {
            AdditionalData = token ?? throw new ArgumentNullException(nameof(token));
        }

        private PushAction(string action, string title, ActionType type, string url)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Type = type;
            Action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void ApplyToNotification(AngularPushNotification notification)
        {
            notification.Actions.Add(new AngularPushNotification.NotificationAction(Action, Title));

            switch (Type)
            {
                case ActionType.Post:
                    notification.Data[$"post-{Action}"] = Url;
                    notification.Data[$"token-{Action}"] = AdditionalData;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unexpected action type");
            }
        }
    }
}
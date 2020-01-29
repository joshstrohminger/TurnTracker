﻿using System;
using System.Linq;
using AutoMapper;
using CSharpFunctionalExtensions;
using Lib.Net.Http.WebPush;
using Microsoft.Extensions.Logging;
using TurnTracker.Data;
using TurnTracker.Data.Entities;
using TurnTracker.Domain.Interfaces;

namespace TurnTracker.Domain.Services
{
    public class PushNotificationService : IPushNotificationService
    {
        #region Fields

        private readonly TurnContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<PushNotificationService> _logger;

        #endregion Fields

        #region Ctor

        public PushNotificationService(TurnContext db, IMapper mapper, ILogger<PushNotificationService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        #endregion Ctor

        #region Public

        public Result SaveSubscription(int userId, PushSubscription sub)
        {
            try
            {
                if (_db.PushSubscriptionDevices.Find(userId, sub.Endpoint) is null)
                {
                    var device = _mapper.Map<PushSubscriptionDevice>(sub);
                    device.UserId = userId;
                    _db.PushSubscriptionDevices.Add(device);
                    _db.SaveChanges();
                }

                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to save sub for user {userId}");
                return Result.Failure("Failed to save subscription");
            }
        }

        public Result RemoveSubscription(int userId, PushSubscription sub)
        {
            try
            {
                var device = _db.PushSubscriptionDevices.Find(userId, sub.Endpoint);
                if (device != null)
                {
                    _db.PushSubscriptionDevices.Remove(device);
                    _db.SaveChanges();
                }
                return Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to delete sub for user {userId}");
                return Result.Failure("Failed to delete subscription");
            }
        }

        public Result SendToOneDevice(int userId, string message, string endpoint)
        {
            var sub = _db.PushSubscriptionDevices.Find(userId, endpoint);
            if (sub is null)
            {
                return Result.Failure("Couldn't find device subscription");
            }

            return Send(sub, message);
        }

        public Result SendToAllDevices(int userId, string message)
        {
            return Result.Combine(
                _db.PushSubscriptionDevices
                    .Where(x => x.UserId == userId)
                    .Select(sub => Send(sub, message)));
        }

        #endregion Public

        #region Private

        private Result Send(PushSubscriptionDevice sub, string message)
        {
            return Result.Failure("not implemented");
        }

        #endregion Private
    }
}
﻿using MSAccountPushSubscription.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MSAccountPushSubscription.Services
{
    interface IPushNotificationService
    {
        Task Subscribe(PushSubscriptionInformation subscription);
        Task UnSubscribe(string endPoint);
    }
}

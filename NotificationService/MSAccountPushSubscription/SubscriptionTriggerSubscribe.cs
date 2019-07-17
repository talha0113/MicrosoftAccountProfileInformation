using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MSAccountPushSubscription.Models;
using Microsoft.Azure.Documents.Client;
using MSAccountPushSubscription.Services;
using MSAccountPushSubscription.Managers;
using System.Collections;

namespace MSAccountPushSubscription
{
    public class SubscriptionTriggerSubscribe
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionTriggerSubscribe(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        [FunctionName("SubscriptionTriggerSubscribe")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Subscriptions",
                collectionName: "Items",
                ConnectionStringSetting = "ms-account-profile-informationDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("SubscriptionTriggerSubscribe Request Started.");
            //foreach (DictionaryEntry de in SettingsManager.GetAll())
            //{
            //    log.LogInformation($"{de.Key}:{de.Value}");
            //}
            //log.LogInformation($"Connection String: {Environment..GetEnvironmentVariable("ConnectionStrings:ms-account-profile-informationDBConnection", EnvironmentVariableTarget.Process)}");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var pushSubscription = JsonConvert.DeserializeObject<PushSubscriptionInformation>(requestBody);

                if (pushSubscription != null)
                {
                    _subscriptionService.Client = client;
                    var service = this._subscriptionService;
                    await service.Subscribe(pushSubscription);
                    return new OkResult();
                }
                else
                {
                    var emptyMessage = "Empty Subscription in the Body";
                    log.LogWarning(emptyMessage);
                    return new BadRequestObjectResult(emptyMessage);
                }
            }
            catch (Exception ex)
            {
                var exception = new ApplicationException("Error Occuered", ex);
                log.LogCritical(exception, exception.Message);
                return new BadRequestObjectResult(exception);
            }
            finally
            {
                log.LogInformation("SubscriptionTriggerSubscribe Request Ended.");
            }
        }
    }
}

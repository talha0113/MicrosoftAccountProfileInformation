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

namespace MSAccountPushSubscription
{
    public static class SubscriptionTriggerSubscribe
    {
        [FunctionName("SubscriptionTriggerSubscribe")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req
            ,
            //[CosmosDB(
            //    databaseName: "MSAccountPushSubscription",
            //    collectionName: "Subscriptions",
            //    ConnectionStringSetting = "MSAccountPushSubscriptionDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("SubscriptionTriggerSubscribe Request Started.");

            try {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var pushSubscription = JsonConvert.DeserializeObject<PushSubscriptionInformation>(requestBody);

                if (pushSubscription != null)
                {
                    var service = new PushNotificationService();
                    service.SendNotification(pushSubscription, JsonConvert.SerializeObject(new RootNotification()));
                    return new OkResult();
                }
                else
                {
                    return new BadRequestObjectResult("Empty Subscription in the Body");
                }
            }
            catch (Exception ex) {
                var exception = new ApplicationException("Error Occuered", ex);
                return new BadRequestObjectResult(exception);
            }
            
        }
    }
}
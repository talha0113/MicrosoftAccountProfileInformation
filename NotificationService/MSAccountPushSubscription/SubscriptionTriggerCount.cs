using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using MSAccountPushSubscription.Services;

namespace MSAccountPushSubscription
{
    public class SubscriptionTriggerCount
    {
        private readonly ISubscriptionService _subscriptionService;
        public SubscriptionTriggerCount(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        [FunctionName("SubscriptionTriggerCount")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "Subscriptions",
                collectionName: "Items",
                ConnectionStringSetting = "ms-account-profile-informationDBConnection")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("SubscriptionTriggerCount Request Started.");

            try
            {
                _subscriptionService.Client = client;
                var service = this._subscriptionService;
                var count = await service.Count();
                return new OkObjectResult(count);
            }
            catch (Exception ex)
            {
                var exception = new ApplicationException("Error Occuered", ex);
                return new BadRequestObjectResult(exception);
            }
            finally
            {
                log.LogInformation("SubscriptionTriggerCount Request Ended.");
            }
        }
    }
}

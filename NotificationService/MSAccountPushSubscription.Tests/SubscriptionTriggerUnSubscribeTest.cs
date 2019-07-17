using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSAccountPushSubscription.Models;
using MSAccountPushSubscription.Repositories;
using MSAccountPushSubscription.Services;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MSAccountPushSubscription.Tests
{
    [TestClass]
    public class SubscriptionTriggerUnSubscribeTest
    {
        private ILogger log;
        private DefaultHttpRequest request;
        private PushSubscriptionInformation sub;
        private DocumentClient client;
        private SubscriptionTriggerUnSubscribe subscriptionTriggerUnSubscribe;
        private SubscriptionService subscriptionService;
        private INotificationQueueService notificationQueueService;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            log = NullLoggerFactory.Instance.CreateLogger("Dummy");

            sub = new PushSubscriptionInformation();
            sub.EndPoint = "https://dummy_endpoint";
            sub.ExpirationTime = null;
            sub.Keys = new Keys();
            sub.Keys.Authentication = "dummy_authentication";
            sub.Keys.p256dh = "dummy_p256dh";

            Environment.SetEnvironmentVariable("DatabaseId", TestContext.Properties["DatabaseId"].ToString(), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("CollectionId", TestContext.Properties["CollectionId"].ToString(), EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", TestContext.Properties["AzureWebJobsStorage"].ToString(), EnvironmentVariableTarget.Process);

            client = new DocumentClient(new Uri(TestContext.Properties["AccountEndpoint"].ToString()), TestContext.Properties["AccountKey"].ToString());
            notificationQueueService = new NotificationQueueService();
            subscriptionService = new SubscriptionService(notificationQueueService);
            subscriptionTriggerUnSubscribe = new SubscriptionTriggerUnSubscribe(subscriptionService);
        }

        [TestMethod]
        public async Task UnSubscribeEmptyQueryTest()
        {
            request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                QueryString = QueryString.Create("", "")
            };
            var response = await subscriptionTriggerUnSubscribe.Run(request, client, log);
            Assert.IsInstanceOfType(response, typeof(BadRequestObjectResult));
            Assert.IsTrue((((BadRequestObjectResult)response).Value as string).Contains("Empty"));
        }

        [TestMethod]
        public async Task SubscriptionExistsTest()
        {
            DocumentDBRepository<PushSubscriptionInformation>.CreateItemAsync(sub).Wait();
            var item = await DocumentDBRepository<PushSubscriptionInformation>.GetItemAsync(sub.Id);
            Assert.AreEqual(item.Id, sub.Id);
        }

        [TestMethod]
        public async Task SubscriptionRemovedTest()
        {
            request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                QueryString = QueryString.Create("endpoint", sub.EndPoint)
            };
            var response = await subscriptionTriggerUnSubscribe.Run(request, client, log);
            Assert.IsInstanceOfType(response, typeof(OkResult));
        }

        public async Task SubscriptionsEmptyTest()
        {
            var item = await DocumentDBRepository<PushSubscriptionInformation>.GetItemAsync(sub.Id);
            Assert.AreEqual(item, null);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            //DocumentDBRepository<PushSubscriptionInformation>.Clean();
        }
    }
}

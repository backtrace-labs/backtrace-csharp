using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Services;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.Events
{
    /// <summary>
    /// Tests BacktraceAPI events 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Events.TestClientEvents", Description = "Test client events")]
    public class ApiEventsTests
    {
        private BacktraceClient _backtraceClient;
        private BacktraceClient _clientWithInvalidParameters;

        [SetUp]
        public void Setup()
        {
            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");
            var invalidCredentials = new BacktraceCredentials("https://validurl.com/", "invalidToken");
            //mock API
            var serverUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            var invalidUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(serverUrl)
                .Respond("application/json", "{'object' : 'aaa'}");

            mockHttp.When(invalidUrl)
                .Respond("application/json", "{'message': 'invalid data'}");

            var api = new Mock<BacktraceApi<object>>(credentials);
            api.Object.HttpClient = mockHttp.ToHttpClient();

            var apiWithInvalidUrl = new Mock<BacktraceApi<object>>(invalidCredentials);
            api.Object.HttpClient = mockHttp.ToHttpClient();

            //mock database
            var database = new Mock<IBacktraceDatabase<object>>();
            database.Setup(n => n.GenerateMiniDump(It.IsAny<BacktraceReportBase<object>>(), It.IsAny<MiniDumpType>()));

            //setup new client
            _backtraceClient = new BacktraceClient(credentials, reportPerMin: 0)
            {
                _backtraceApi = api.Object,
                _database = database.Object
            };
            _clientWithInvalidParameters = new BacktraceClient(invalidCredentials, reportPerMin: 0)
            {
                _backtraceApi = apiWithInvalidUrl.Object,
                _database = database.Object
            };
        }

        [Test(Author = "Konrad Dysput", Description = "Test valid report submission")]
        public async Task TestValidSubmissionEvents()
        {
            bool responseEventTrigger = false;
            int totalResponses = 0;
            _backtraceClient.OnServerResponse = (BacktraceResult res) =>
            {
                totalResponses++;
                responseEventTrigger = true;
                Assert.AreEqual(res.Object, "aaa");
            };
            await _backtraceClient.SendAsync("custom message");
            await _backtraceClient.SendAsync(new Exception("Backtrace API tests"));
            await _backtraceClient.SendAsync(new BacktraceReport("backtrace report message"));
            await _backtraceClient.SendAsync(new BacktraceReport(new Exception("Backtrace report exception")));

            Assert.IsTrue(responseEventTrigger);
            Assert.AreEqual(totalResponses, 4);
        }

        [Test(Author = "Konrad Dysput", Description = "Test invalid report submission")]
        public async Task TestInvalidSubmissionEvents()
        {
            bool responseEventTrigger = false;
            int totalResponses = 0;
            _clientWithInvalidParameters.OnServerError = (Exception e) =>
            {
                totalResponses++;
                responseEventTrigger = true;
            };

            await _clientWithInvalidParameters.SendAsync("custom message");
            await _clientWithInvalidParameters.SendAsync(new Exception("Backtrace API tests"));
            await _clientWithInvalidParameters.SendAsync(new BacktraceReport("backtrace report message"));
            await _clientWithInvalidParameters.SendAsync(new BacktraceReport(new Exception("Backtrace report exception")));

            Assert.IsTrue(responseEventTrigger);
            Assert.AreEqual(totalResponses, 4);
        }



    }
}

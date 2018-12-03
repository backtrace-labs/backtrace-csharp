using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtrace.Tests.IntegrationTests
{
    [TestFixture(Author = "Arthur Tu", Category = "IntegrationTests.ClientRateLimit", Description = "Test rate limiting with diffrent threads")]
    public class ClientReportLimitTests
    {
        /// <summary>
        /// Backtrace client
        /// </summary>
        private BacktraceClient _backtraceClient;
        /// <summary>
        /// Information about client report limit
        /// </summary>
        private bool clientReportLimitReached = false;

        /// <summary>
        /// Prepare basic setup of Backtrace client
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //prepare mock object
            //mock database
            var database = new Mock<IBacktraceDatabase>();
            database.Setup(n =>
                n.Add(It.IsAny<BacktraceReport>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<MiniDumpType>()));

            database.Setup(n =>
               n.Delete(It.IsAny<BacktraceDatabaseRecord>()));

            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");

            //mock api
            var serverUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(serverUrl)
                .Respond("application/json", "{'object' : 'aaa'}");
            var api = new BacktraceApi(credentials, 0)
            {
                HttpClient = mockHttp.ToHttpClient()
            };

            //setup new client
            _backtraceClient = new BacktraceClient(credentials, database: database.Object)
            {
                BacktraceApi = api
            };

            //Add new scoped attributes
            _backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            _backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            _backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "BacktraceIntegrationTest",
                Type = "Library"
            };
            _backtraceClient.Attributes["ComplexObject"] = new Dictionary<string, Uri>()
            {
                {"backtrace.io" , new Uri("http://backtrace.io") },
                {"Google url" , new Uri("http://google.com") }
            };
            //to check if client report limit reached use OnClientReportLimitReached 
            _backtraceClient.OnClientReportLimitReached = (BacktraceReport report) =>
            {
                clientReportLimitReached = true;
            };
        }

        private void DivideByZeroMethod()
        {

            int x = 0;
            var result = 5 / x;
        }

        private void OutOfRangeMethod()
        {
            int[] x = new int[1];
            x[1] = 1 - 1;
        }

        private async Task ThreadTest(int threadIndex)
        {
            await _backtraceClient.SendAsync($"Custom client message");
            try
            {
                DivideByZeroMethod();
            }
            catch (DivideByZeroException divideException)
            {
                await _backtraceClient.SendAsync(divideException);
            }
            try
            {
                OutOfRangeMethod();
            }
            catch (IndexOutOfRangeException outOfRangeException)
            {
                await _backtraceClient.SendAsync(outOfRangeException);
            }
            await _backtraceClient.SendAsync($"End test case for thread {threadIndex}");
        }

        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Test(Author = "Arthur Tu and Konrad Dysput", Description = "Test rate limiting on single/multiple thread thread")]
        public void SingleThreadWithoutReportRateLimit(int numberOfTasks)
        {
            // one thread = 4 request to API 
            int expectedNumberOfReports = numberOfTasks * 4;
            int totalSend = 0;

            //set rate limiting to unlimite
            _backtraceClient.SetClientReportLimit(0);
            clientReportLimitReached = false;
            _backtraceClient.AfterSend = (BacktraceResult res) =>
            {
                totalSend++;
            };

            //prepare thread and catch 2 exception per thread and send two custom messages
            var taskList = new Task[numberOfTasks];
            for (int threadIndex = 0; threadIndex < numberOfTasks; threadIndex++)
            {
                taskList[threadIndex] = ThreadTest(threadIndex);
            }
            Task.WaitAll(taskList);

            Assert.AreEqual(expectedNumberOfReports, totalSend);
            Assert.IsFalse(clientReportLimitReached);
        }
    }
}

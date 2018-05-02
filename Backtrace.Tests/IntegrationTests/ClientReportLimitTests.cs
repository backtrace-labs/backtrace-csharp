using Backtrace.Base;
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
            var database = new Mock<IBacktraceDatabase<object>>();
            database.Setup(n =>
                n.Add(It.IsAny<BacktraceReportBase<object>>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<MiniDumpType>()));

            database.Setup(n =>
               n.Delete(It.IsAny<BacktraceDatabaseEntry<object>>()));

            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");

            //mock api
            var serverUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(serverUrl)
                .Respond("application/json", "{'object' : 'aaa'}");
            var api = new BacktraceApi<object>(credentials, 0, false)
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

        /// <summary>
        /// Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting
        /// </summary>
        [TestCase(1, 2)]
        [TestCase(5, 2)]
        [TestCase(10, 2)]
        [TestCase(1, 5)]
        [TestCase(5, 10)]
        [TestCase(5, 20)]
        [Test(Author = "Arthur Tu and Konrad Dysput", Description = "Test a initialization and submission sequence for backtrace client w/ threading w/o rate limiting")]
        public void ThreadedWithReportRateLimit(int numberOfTasks, int clientRateLimit)
        {
            //set rate limiting
            clientReportLimitReached = false;
            _backtraceClient.SetClientReportLimit((uint)clientRateLimit);

            //set expected number of drop and request
            int expectedNumberOfAttempts = 4 * numberOfTasks;
            int expectedNumberOfDropRequest = expectedNumberOfAttempts - (int)clientRateLimit;

            if (expectedNumberOfDropRequest < 0)
            {
                expectedNumberOfDropRequest = 0;
            }

            var tasks = new Task[numberOfTasks];
            int totalAttemps = 0;
            int totalDrop = 0;
            int totalNumberOfDropsOnEvents = 0;

            //set backtrace events
            _backtraceClient.OnClientReportLimitReached = (BacktraceReport report) =>
            {
                totalDrop++;
                clientReportLimitReached = true;
            };
            _backtraceClient.AfterSend = (BacktraceResult res) =>
            {
                if (res.Status == BacktraceResultStatus.LimitReached)
                {
                    totalNumberOfDropsOnEvents++;
                }
                totalAttemps++;
            };

            //initialize startup tasks
            for (int taskIndex = 0; taskIndex < numberOfTasks; taskIndex++)
            {
                tasks[taskIndex] = ThreadTest(taskIndex);
            }
            Task.WaitAll(tasks);

            //check if BacktraceResult is correct on events
            Assert.AreEqual(totalDrop, totalNumberOfDropsOnEvents);
            //check correct number of attempts
            Assert.AreEqual(totalAttemps, expectedNumberOfAttempts);
            //check if expected number of drops are equal to total dropped packages from rate limit client
            Assert.AreEqual(totalDrop, expectedNumberOfDropRequest);
            //check if limit reached or if any report was dropped
            Assert.IsTrue(clientReportLimitReached || totalDrop == 0);
        }
    }
}

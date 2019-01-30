﻿using Backtrace.Base;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backtrace.Tests.Events
{
    /// <summary>
    /// Tests all available BacktraceClient events 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Events.TestClientEvents", Description = "Test client events")]
    public class EventTests
    {
        /// <summary>
        /// BacktraceClient
        /// </summary>
        private BacktraceClient _backtraceClient;

        /// <summary>
        /// Prepare BacktraceClient
        /// </summary>
        [SetUp]
        public void Setup()
        {
            //prepare mock object
            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");
            //mock api
            var serverUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(serverUrl)
                .Respond("application/json", "{'object' : 'aaa'}");

            var api = new BacktraceApi(credentials, 0)
            {
                HttpClient = mockHttp.ToHttpClient(),
                //avoid real submission
                RequestHandler = (string host, string boundaryId, BacktraceData data) =>
                {
                    return new BacktraceResult();
                }
            };

            //mock database
            var database = new Mock<IBacktraceDatabase>();
            database.Setup(n =>
                n.Add(It.IsAny<BacktraceReport>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<MiniDumpType>()));

            database.Setup(n =>
               n.Delete(It.IsAny<BacktraceDatabaseRecord>()));


            //setup new client
            _backtraceClient = new BacktraceClient(credentials, database: database.Object, reportPerMin: 0)
            {
                BacktraceApi = api
            };
        }

        [TestCase(5, 2)]
        [TestCase(15, 5)]
        [Test(Author = "Konrad Dysput", Description = "Test rate limiting and after send event")]
        public async Task TestClientRateLimiting(int numberOfCycles, int rateLimit)
        {
            //we send reports by using Send method and SendAsync method - 8 per c
            int totalSendReport = numberOfCycles * 8;
            int expectedNumberOfReports = rateLimit > totalSendReport ? totalSendReport : rateLimit;
            int totalNumberOfReports = 0;
            bool eventTrigger = false;

            _backtraceClient.OnClientReportLimitReached = (BacktraceReport report) =>
            {
                eventTrigger = true;
            };

            _backtraceClient.AfterSend = (BacktraceResult res) =>
            {
                if (res.Status == BacktraceResultStatus.Ok)
                {
                    totalNumberOfReports++;
                }
            };

            _backtraceClient.SetClientReportLimit((uint)rateLimit);
            for (int i = 0; i < numberOfCycles; i++)
            {
                //test sync submission
                _backtraceClient.Send("client message");
                _backtraceClient.Send(new Exception("test exception"));
                _backtraceClient.Send(new BacktraceReport("report message"));
                _backtraceClient.Send(new BacktraceReport(new Exception("test exception")));
                //test async submission
                await _backtraceClient.SendAsync("client message");
                await _backtraceClient.SendAsync(new Exception("test exception"));
                await _backtraceClient.SendAsync(new BacktraceReport("report message"));
                await _backtraceClient.SendAsync(new BacktraceReport(new Exception("test exception")));
            }

            Assert.IsTrue(eventTrigger);
            Assert.AreEqual(expectedNumberOfReports, totalNumberOfReports);
        }

        /// <summary>
        /// Test OnReportStart, BeforeSend and AfterSend events
        /// </summary>
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Test(Author = "Konrad Dysput", Description = "Test OnReportStart, BeforeSend and AfterSend events")]
        public async Task TestStartAndStopEvents(int numberOfThreads)
        {
            //setup test method
            //Test case tests Send and SendAsync method - magic number 2 means that we test 2 methods
            var expectedNumberOfStarts = numberOfThreads * 2;
            var expectedNumberOfEnds = numberOfThreads * 2;
            var expectedNumberOfBeforeSendEvents = numberOfThreads * 2;

            List<Task> threads = new List<Task>();
            int totalStart = 0;
            int totalBeforeSend = 0;
            int totalEnds = 0;


/* Unmerged change from project 'Backtrace.Tests(net45)'
Before:
            _backtraceClient.OnReportStart = (BacktraceReport report) =>
After:
            _backtraceClient.OnReportStart = (Model.BacktraceReport report) =>
*/
            this._backtraceClient.OnReportStart = (global::Backtrace.Model.BacktraceReport report) =>
            {
                Interlocked.Increment(ref totalStart);
            };
            _backtraceClient.BeforeSend = (BacktraceData data) =>
            {
                Interlocked.Increment(ref totalBeforeSend);
                return data;
            };
            _backtraceClient.AfterSend = (BacktraceResult result) =>
            {
                Interlocked.Increment(ref totalEnds);
            };

            for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
            {

                threads.Add(Task.Run(async () =>
                {
                    _backtraceClient.Send("client message");
                    await _backtraceClient.SendAsync("client message");
                }));
            }

            await Task.WhenAll(threads);

            Assert.AreEqual(expectedNumberOfStarts, totalStart, "Start event count doesn't match");
            Assert.AreEqual(expectedNumberOfBeforeSendEvents, totalBeforeSend, "Before send event count doesn't match");
            Assert.AreEqual(expectedNumberOfEnds, totalEnds, "After send event count doesn't match");
        }
    }
}

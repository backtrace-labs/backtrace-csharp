using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
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
            //mock api
            var api = new Mock<IBacktraceApi<object>>();
            api.Setup(n => n.SendAsync(It.IsAny<BacktraceData<object>>())).Returns(Task.FromResult(new BacktraceResult()));
            api.Setup(n => n.Send(It.IsAny<BacktraceData<object>>())).Returns(new BacktraceResult());

            //mock database
            var database = new Mock<IBacktraceDatabase<object>>();
            database.Setup(n => n.GenerateMiniDump(It.IsAny<BacktraceReportBase<object>>(), It.IsAny<MiniDumpType>()));

            //setup new client
            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");
            _backtraceClient = new BacktraceClient(credentials, reportPerMin: 0)
            {
                _backtraceApi = api.Object,
                _database = database.Object
            };
        }

        /// <summary>
        /// Test OnReportStart and AfterSend events
        /// </summary>
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(10)]
        [Test(Author = "Konrad Dysput", Description = "Test OnReportStart and AfterSend events")]
        public void TestStartAndStopEvents(int numberOfThreads)
        {
            //setup test method
            //Test case tests Send and SendAsync method - magic number 2 means that we test 2 methods
            var expectedNumberOfStarts = numberOfThreads * 2;
            var expectedNumberOfEnds = numberOfThreads * 2;
            var expectedNumberOfBeforeSendEvents = numberOfThreads * 2;

            List<Thread> threads = new List<Thread>();
            int totalStart = 0;
            int totalBeforeSennd = 0;
            int totalEnds = 0;

            _backtraceClient.OnReportStart = (BacktraceReport report) =>
            {
                totalStart++;
            };
            _backtraceClient.BeforeSend = (BacktraceData<object> data) =>
            {
                totalBeforeSennd++;
                return data;
            };
            _backtraceClient.AfterSend = (BacktraceResult result) =>
            {
                totalEnds++;
            };

            for (int threadIndex = 0; threadIndex < numberOfThreads; threadIndex++)
            {

                threads.Add(new Thread(new ThreadStart(() =>
                {
                    _backtraceClient.Send("client message");
                    _backtraceClient.SendAsync("client message").Wait();
                })));
            }

            threads.ForEach(n => n.Start());
            threads.ForEach(n => n.Join());

            Assert.AreEqual(totalStart, expectedNumberOfStarts);
            Assert.AreEqual(totalBeforeSennd, expectedNumberOfBeforeSendEvents);
            Assert.AreEqual(totalEnds, expectedNumberOfEnds);
        }
    }
}

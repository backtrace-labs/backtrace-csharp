using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Tests.DatabaseTests.Model;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    // <summary>
    /// Test timer events
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.DatabaseFlow.LockingObjects")]
    public class DatabaseEventBehaviourTests
    {
        /// <summary>
        /// Backtrace client
        /// </summary>
        private BacktraceClient _backtraceClient;

        /// <summary>
        /// Backtrace database
        /// </summary>
        private BacktraceDatabase _backtraceDatabase;

        /// <summary>
        /// Last database entry
        /// </summary>
        BacktraceDatabaseEntry _lastEntry;

        /// <summary>
        /// Get new database entry 
        /// </summary>
        /// <returns>Database entry mock</returns>
        protected BacktraceDatabaseEntry GetEntry()
        {
            //mock single entry
            var mockEntry = new Mock<BacktraceDatabaseEntry>();
            mockEntry.Setup(n => n.Delete());
            mockEntry.SetupProperty(n => n.Entry, null);

            //mock entry writer
            mockEntry.Object.EntryWriter = new MockBacktraceDatabaseWriter();
            return mockEntry.Object;
        }

        [SetUp]
        public void Setup()
        {
            _lastEntry = GetEntry();
            //get project path
            string projectPath = Environment.CurrentDirectory;
            //setup credentials
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

            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.GetEntries())
                .Returns(new List<FileInfo>());
            mockFileContext.Setup(n => n.RemoveOrphaned(It.IsAny<IEnumerable<BacktraceDatabaseEntry>>()));

            _backtraceDatabase = new BacktraceDatabase(new BacktraceDatabaseSettings(projectPath)
            {
                AutoSendMode = true,
                RetryBehavior = RetryBehavior.ByInterval
            })
            {
                BacktraceDatabaseFileContext = mockFileContext.Object,
            };

            //setup new client
            _backtraceClient = new BacktraceClient(backtraceCredentials: credentials, database: _backtraceDatabase, reportPerMin: 0)
            {
                BacktraceApi = api
            };
            _backtraceClient.MiniDumpType = MiniDumpType.None;
        }

        [Test(Author = "Konrad Dysput", Description = "Test retrieving objects from Database when Send method lock entry")]
        public async Task TestRetrievingReporsLockedInSendMethod()
        {
            // BacktraceClient triggers BeforeSend event after retrieving entry from database
            // in this case entry should be locked
            // BacktraceDatabase shouldn't try to send report from timer event because, all entries all in use (1)
            _backtraceClient.BeforeSend = (BacktraceData data) =>
            {
                var dbEntries = _backtraceDatabase.BacktraceDatabaseContext.FirstOrDefault();
                Assert.IsNull(dbEntries);
                return data;
            };

            await _backtraceClient.SendAsync(new BacktraceReport("test message"));
        }
    }
}
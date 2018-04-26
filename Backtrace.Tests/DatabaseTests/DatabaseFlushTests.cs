using Backtrace.Interfaces;
using Backtrace.Extensions;
using Backtrace.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backtrace.Types;
using System.IO;
using Backtrace.Model.Database;
using Backtrace.Base;

namespace Backtrace.Tests.DatabaseTests
{
    /// <summary>
    /// Test flush/flush async 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Flush")]
    public class DatabaseFlushTests
    {
        /// <summary>
        /// Database
        /// </summary>
        private BacktraceDatabase<object> _database;

        [SetUp]
        public void Setup()
        {
            //get project path
            string projectPath = Environment.CurrentDirectory;

            //mock api
            var mockApi = new Mock<IBacktraceApi<object>>();
            mockApi.Setup(n => n.Send(It.IsAny<BacktraceData<object>>()))
                .Returns(new BacktraceResult());

            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext<object>>();
            mockFileContext.Setup(n => n.GetEntries())
                .Returns(new List<FileInfo>());

            //mock cache
            var mockCacheContext = new Mock<IBacktraceDatabaseContext<object>>();
            mockFileContext.Setup(n => n.RemoveOrphaned(It.IsAny<IEnumerable<BacktraceDatabaseEntry<object>>>()));

            _database = new BacktraceDatabase<object>(projectPath)
            {
                BacktraceDatabaseContext = new MockBacktraceDatabaseContext(projectPath, 3, RetryOrder.Stack),
                BacktraceDatabaseFileContext = mockFileContext.Object,
                BacktraceApi = mockApi.Object
            };
        }

        private BacktraceDatabaseEntry<object> GetEntry()
        {
            //mock single entry
            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Delete());
            mockEntry.Setup(n => n.BacktraceData)
                .Returns(new BacktraceData<object>(It.IsAny<BacktraceReportBase<object>>(), It.IsAny<Dictionary<string, object>>()));

            return mockEntry.Object;
        }

        [Test(Author = "Konrad Dysput", Description = "Test database flush method")]
        public void TestSyncFlushMethod()
        {
            int expectedNumberOfEntries = 10;
           
            for (int i = 0; i < expectedNumberOfEntries; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
            ((MockBacktraceDatabaseContext)_database.BacktraceDatabaseContext).DisposeUsedFiles();

            Assert.AreEqual(expectedNumberOfEntries, _database.Count());
            _database.Flush();
            Assert.AreNotEqual(expectedNumberOfEntries, _database.Count());
            Assert.AreEqual(_database.Count(), 0);
        }


        [Test(Author = "Konrad Dysput", Description = "Test database flush async method")]
        public async Task TestFlushAsyncMethods()
        {
            int expectedNumberOfEntries = 10;

            for (int i = 0; i < expectedNumberOfEntries; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
          ((MockBacktraceDatabaseContext)_database.BacktraceDatabaseContext).DisposeUsedFiles();

            Assert.AreEqual(expectedNumberOfEntries, _database.Count());
            await _database.FlushAsync();
            Assert.AreNotEqual(expectedNumberOfEntries, _database.Count());
            Assert.AreEqual(_database.Count(), 0);
        }
    }
}

using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Backtrace.Tests.DatabaseTests.Model
{
    /// <summary>
    /// Database tests base class
    /// </summary>
    public class DatabaseTestBase
    {
        /// <summary>
        /// Database
        /// </summary>
        protected BacktraceDatabase _database;

        [SetUp]
        public virtual void Setup()
        {
            //get project path
            string projectPath = Path.GetTempPath();

            //mock api
            var mockApi = new Mock<IBacktraceApi>();
            mockApi.Setup(n => n.Send(It.IsAny<BacktraceData>()))
                .Returns(new BacktraceResult());

            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.GetRecords())
                .Returns(new List<FileInfo>());

            //mock cache
            var mockCacheContext = new Mock<IBacktraceDatabaseContext>();
            mockFileContext.Setup(n => n.RemoveOrphaned(It.IsAny<IEnumerable<BacktraceDatabaseRecord>>()));
            mockFileContext.Setup(n => n.Clear());

            var settings = new BacktraceDatabaseSettings(projectPath)
            {
                AutoSendMode = false, //we don't want to test timers
                MaxRecordCount = 100,
                RetryLimit = 3
            };
            _database = new BacktraceDatabase(settings)
            {
                BacktraceDatabaseContext = new MockBacktraceDatabaseContext(projectPath, 3, RetryOrder.Stack),
                BacktraceDatabaseFileContext = mockFileContext.Object,
                BacktraceApi = mockApi.Object
            };
        }

        /// <summary>
        /// Dispose all records in memory cache. Use this method only for testing purpose!
        /// </summary>
        protected void DisposeRecords()
        {
            ((MockBacktraceDatabaseContext)_database.BacktraceDatabaseContext).DisposeUsedFiles();
        }

        protected void ChangeRetryOrder(RetryOrder @newOrder)
        {
            ((BacktraceDatabaseContext)_database.BacktraceDatabaseContext).RetryOrder = newOrder;
        }

        /// <summary>
        /// Get new database record
        /// </summary>
        /// <returns>Database record mock</returns>
        protected BacktraceDatabaseRecord GetRecord()
        {
            //mock single record
            var mockRecord = new Mock<BacktraceDatabaseRecord>();
            mockRecord.Setup(n => n.Delete());
            mockRecord.Setup(n => n.BacktraceData)
                .Returns(new BacktraceData(It.IsAny<BacktraceReport>(), It.IsAny<Dictionary<string, object>>()));
            mockRecord.Setup(n => n.Valid())
                .Returns(true);

            var data = new BacktraceData(null, new Dictionary<string, object>());
            mockRecord.SetupProperty(n => n.Record, data);

            mockRecord.Object.RecordWriter = new MockBacktraceDatabaseWriter();
            return mockRecord.Object;
        }
    }
}

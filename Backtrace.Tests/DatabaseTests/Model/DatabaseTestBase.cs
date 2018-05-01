﻿using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Interfaces.Database;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected BacktraceDatabase<object> _database;

        /// <summary>
        /// Entry writer
        /// </summary>
        internal Mock<IBacktraceDatabaseEntryWriter> _entryWriter;

        [SetUp]
        public virtual void Setup()
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

            _entryWriter = new Mock<IBacktraceDatabaseEntryWriter>();

            _database = new BacktraceDatabase<object>(projectPath)
            {
                BacktraceDatabaseContext = new MockBacktraceDatabaseContext(projectPath, 3, RetryOrder.Stack),
                BacktraceDatabaseFileContext = mockFileContext.Object,
                BacktraceApi = mockApi.Object
            };
        }

        /// <summary>
        /// Dispose all entries in memory cache. Use this method only for testing purpose!
        /// </summary>
        protected void DisposeEntries()
        {
            ((MockBacktraceDatabaseContext)_database.BacktraceDatabaseContext).DisposeUsedFiles();
        }

        protected void ChangeRetryOrder(RetryOrder @newOrder)
        {
            ((BacktraceDatabaseContext<object>)_database.BacktraceDatabaseContext).RetryOrder = newOrder;
        }

        /// <summary>
        /// Get new database entry 
        /// </summary>
        /// <returns>Database entry mock</returns>
        protected BacktraceDatabaseEntry<object> GetEntry()
        {
            //mock single entry
            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Delete());
            mockEntry.Setup(n => n.BacktraceData)
                .Returns(new BacktraceData<object>(It.IsAny<BacktraceReportBase<object>>(), It.IsAny<Dictionary<string, object>>()));
            mockEntry.Object._entryWriter = _entryWriter.Object;
            return mockEntry.Object;
        }
    }
}
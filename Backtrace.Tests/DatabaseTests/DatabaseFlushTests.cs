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
using Backtrace.Tests.DatabaseTests.Model;

namespace Backtrace.Tests.DatabaseTests
{
    /// <summary>
    /// Test flush/flush async 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Flush")]
    public class DatabaseFlushTests : DatabaseTestBase
    {
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

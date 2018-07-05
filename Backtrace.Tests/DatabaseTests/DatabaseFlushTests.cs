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
        [TestCase(1)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test database flush method")]
        public void TestSyncFlushMethod(int expectedNumberOfRecords)
        {
            _database.Clear();
            for (int i = 0; i < expectedNumberOfRecords; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
            DisposeRecords();

            Assert.AreEqual(expectedNumberOfRecords, _database.Count());
            _database.Flush();
            Assert.AreNotEqual(expectedNumberOfRecords, _database.Count());
            Assert.AreEqual(_database.Count(), 0);
        }

        [TestCase(1)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test database flush async method")]
        public async Task TestFlushAsyncMethods(int expectedNumberOfRecords)
        {
            _database.Clear();
            for (int i = 0; i < expectedNumberOfRecords; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
            DisposeRecords();

            Assert.AreEqual(expectedNumberOfRecords, _database.Count());
            await _database.FlushAsync();
            Assert.AreNotEqual(expectedNumberOfRecords, _database.Count());
            Assert.AreEqual(_database.Count(), 0);
        }
    }
}

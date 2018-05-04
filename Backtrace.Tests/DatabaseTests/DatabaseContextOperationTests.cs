using Backtrace.Interfaces;
using Backtrace.Model.Database;
using Backtrace.Tests.DatabaseTests.Model;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    /// <summary>
    /// Test cache operations
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Context")]
    public class DatabaseContextOperationTests : DatabaseTestBase
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [Test(Author = "Konrad Dysput", Description = "Test add operation")]
        public void TestNewContextEntries(int entryNumber)
        {
            var totalNumberOfEntries = _database.Count();
            for (int i = 0; i < entryNumber; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
            Assert.AreEqual(totalNumberOfEntries + entryNumber, _database.Count());
        }
        [Test(Author = "Konrad Dysput", Description = "Test maximum number of entries")]
        public void TestMaximumNumberOfEntries()
        {
            _database.Clear();
            //we set maximum number of entries to equal to 100 in Setup method on DatabaseTestBase class
            int maximumNumberOfEntries = 101;
            //we add 100 entries - 100 is our database limit
            for (int i = 0; i < maximumNumberOfEntries - 1; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
            _database.Start();
            Assert.Throws<ArgumentException>(() => _database.Add(null, new Dictionary<string, object>(), MiniDumpType.None));
        }


        [Test(Author = "Konrad Dysput", Description = "Test FIFO retry order")]
        public void TestFifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Queue);
            //mock two entries
            var firstEntry = GetEntry();
            var secoundEntry = GetEntry();
            //test if first entry is a real first entry
            _database.BacktraceDatabaseContext.Add(firstEntry);
            //test received entry after first pop
            _database.BacktraceDatabaseContext.Add(secoundEntry);

            for (int i = 0; i < 10; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
            DisposeEntries();
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, firstEntry.Id);
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, secoundEntry.Id);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(15)]
        [Test(Author = "Konrad Dysput", Description = "Test FIFO list retry order")]
        public void TestFifoListOrder(int totalNumberOfEntries)
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Queue);
            var entries = new List<BacktraceDatabaseEntry<object>>();
            for (int i = 0; i < totalNumberOfEntries; i++)
            {
                var entry = GetEntry();
                entries.Add(entry);
                _database.BacktraceDatabaseContext.Add(entry);
            }
            DisposeEntries();
            foreach (var entry in entries)
            {
                var firstEntryId = _database.BacktraceDatabaseContext.FirstOrDefault().Id;
                Assert.AreEqual(firstEntryId, entry.Id);
            }
        }

        [Test(Author = "Konrad Dysput", Description = "Test LIFO retry order")]
        public void TestLifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Stack);
            for (int i = 0; i < 10; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
            //mock two entries
            var firstEntry = GetEntry();
            var secoundEntry = GetEntry();
            //test if first entry is a real first entry
            _database.BacktraceDatabaseContext.Add(firstEntry);
            //test received entry after first pop
            _database.BacktraceDatabaseContext.Add(secoundEntry);

            DisposeEntries();
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, secoundEntry.Id);
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, firstEntry.Id);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(15)]
        [Test(Author = "Konrad Dysput", Description = "Test LIFO list retry order")]
        public void TestLifoListOrder(int totalNumberOfEntries)
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Stack);
            var entries = new List<BacktraceDatabaseEntry<object>>();
            for (int i = 0; i < totalNumberOfEntries; i++)
            {
                var entry = GetEntry();
                entries.Add(entry);
                _database.BacktraceDatabaseContext.Add(entry);
            }
            DisposeEntries();
            for (int retryIndex = entries.Count - 1; retryIndex >= 0; retryIndex--)
            {
                var entry = entries[retryIndex];
                var firstEntryId = _database.BacktraceDatabaseContext.FirstOrDefault().Id;
                Assert.AreEqual(firstEntryId, entry.Id);
            }
        }


    }
}

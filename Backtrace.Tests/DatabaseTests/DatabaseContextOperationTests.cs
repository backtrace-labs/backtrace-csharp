using Backtrace.Interfaces;
using Backtrace.Model.Database;
using Backtrace.Tests.DatabaseTests.Model;
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

        [Test(Author = "Konrad Dysput", Description = "Test FIFO retry order")]
        public void TestFifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(Types.RetryOrder.Queue);
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

        [Test(Author = "Konrad Dysput", Description = "Test LIFO retry order")]
        public void TestLifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(Types.RetryOrder.Stack);
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

    }
}

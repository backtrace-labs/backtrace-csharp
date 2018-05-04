using Backtrace.Tests.DatabaseTests.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    /// <summary>
    /// Test cache batches
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Batch")]
    public class BatchTests : DatabaseTestBase
    {
        /// <summary>
        /// Add entry to first batch on context
        /// </summary>
        private void AddEntries(int numberOfEntriesOnBatch)
        {
            for (int i = 0; i < numberOfEntriesOnBatch; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetEntry());
            }
        }
        [TestCase(3, 2, 0)]
        [TestCase(5, 5, 5)]
        [TestCase(1, 2, 3)]
        [TestCase(2, 0, 10)]
        [Test(Author = "Konrad Dysput", Description = "Test batch configuration")]
        public void TestBatchAdd(int entriesOnFirstBatch, int entriesOnSecoundBatch, int entriesOnThirdBatch)
        {
            //Set first batch (destination: Third batch)
            AddEntries(entriesOnThirdBatch);
            var totalEntries = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalEntries, entriesOnThirdBatch);

            //move first batch to secound and add new entries to first batch
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            //set new first batch (destination: secound batch)
            AddEntries(entriesOnSecoundBatch);
            totalEntries = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalEntries, entriesOnSecoundBatch + entriesOnThirdBatch);

            //move rest batches and set first batch
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            AddEntries(entriesOnFirstBatch);
            totalEntries = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalEntries, entriesOnFirstBatch + entriesOnSecoundBatch + entriesOnThirdBatch);

            //test batch remove
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            totalEntries = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalEntries, entriesOnFirstBatch + entriesOnSecoundBatch);
        }
    }
}

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
        /// Add record to first batch on context
        /// </summary>
        private void AddRecords(int numberOfRecordsOnBatch)
        {
            for (int i = 0; i < numberOfRecordsOnBatch; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
        }
        [TestCase(3, 2, 0)]
        [TestCase(5, 5, 5)]
        [TestCase(1, 2, 3)]
        [TestCase(2, 0, 10)]
        [Test(Author = "Konrad Dysput", Description = "Test batch configuration")]
        public void TestBatchAdd(int recordsOnFirstBatch, int recordsOnSecoundBatch, int recordsOnThirdBatch)
        {
            //Set first batch (destination: Third batch)
            AddRecords(recordsOnThirdBatch);
            var totalRecords = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalRecords, recordsOnThirdBatch);

            //move first batch to secound and add new records to first batch
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            //set new first batch (destination: secound batch)
            AddRecords(recordsOnSecoundBatch);
            totalRecords = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalRecords, recordsOnSecoundBatch + recordsOnThirdBatch);

            //move rest batches and set first batch
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            AddRecords(recordsOnFirstBatch);
            totalRecords = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalRecords, recordsOnFirstBatch + recordsOnSecoundBatch + recordsOnThirdBatch);

            //test batch remove
            _database.BacktraceDatabaseContext.IncrementBatchRetry();
            totalRecords = _database.BacktraceDatabaseContext.Count();
            Assert.AreEqual(totalRecords, recordsOnFirstBatch + recordsOnSecoundBatch);
        }
    }
}

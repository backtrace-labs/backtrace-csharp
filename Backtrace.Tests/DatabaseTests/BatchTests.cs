using Backtrace.Tests.DatabaseTests.Model;
using NUnit.Framework;

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
        private void AddRecords(int numberOfRecordsOnBatch, bool locked = false)
        {
            for (int i = 0; i < numberOfRecordsOnBatch; i++)
            {
                var fakeRecord = _database.BacktraceDatabaseContext.Add(GetRecord());
                fakeRecord.Locked = locked;
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

        [Test]
        public void TestRecordLimitInBatches()
        {
            _database.Start();
            _database.Clear();
            //value from database settings
            int maxRecordCount = 100;
            AddRecords(maxRecordCount);

            var report = new Backtrace.Model.BacktraceReport("report");
            var result = _database.Add(
                backtraceReport: report,
                attributes: null,
                miniDumpType: Types.MiniDumpType.None
                );
            Assert.IsNotNull(result);
            Assert.AreEqual(maxRecordCount, _database.Count());
        }

        [Test]
        public void TestRecordLimitWithAllReservedBatches()
        {
            _database.Start();
            _database.Clear();
            //value from database settings
            int maxRecordCount = 100;
            AddRecords(maxRecordCount, true);

            var report = new Backtrace.Model.BacktraceReport("report");
            var result = _database.Add(
                backtraceReport: report,
                attributes: null,
                miniDumpType: Types.MiniDumpType.None
                );
            Assert.IsNull(result);
            Assert.AreEqual(maxRecordCount, _database.Count());
        }
    }
}

using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Tests.DatabaseTests.Model;
using Backtrace.Types;
using NUnit.Framework;
using System.Collections.Generic;

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
        public void TestNewContextRecords(int recordNumber)
        {
            var totalNumberOfRecords = _database.Count();
            for (int i = 0; i < recordNumber; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
            Assert.AreEqual(totalNumberOfRecords + recordNumber, _database.Count());
        }
        [Test(Author = "Konrad Dysput", Description = "Test maximum number of records")]
        public void TestMaximumNumberOfRecords()
        {
            _database.Clear();
            //we set maximum number of records  to equal to 100 in Setup method on DatabaseTestBase class
            int maximumNumberOfRecords = 100;
            //we add 100 records - 100 is our database limit
            for (int i = 0; i < maximumNumberOfRecords; i++)
            {
                var fakeRecord = GetRecord();
                var record = _database.BacktraceDatabaseContext.Add(fakeRecord);
                fakeRecord.Locked = false;
            }
            _database.Start();
            _database.Add(
                backtraceReport: new BacktraceReport("fake report"),
                attributes: new Dictionary<string, object>(),
                miniDumpType: MiniDumpType.None);

            // in the end BacktraceDatabase should contain 100 reports. 
            // Database should remove first ever report.
            Assert.AreEqual(_database.Count(), maximumNumberOfRecords);
        }


        [Test(Author = "Konrad Dysput", Description = "Test FIFO retry order")]
        public void TestFifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Queue);
            //mock two records
            var firstRecord = GetRecord();
            var secoundRecord = GetRecord();
            //test if first record is a real first record
            _database.BacktraceDatabaseContext.Add(firstRecord);
            //test received record after first pop
            _database.BacktraceDatabaseContext.Add(secoundRecord);

            for (int i = 0; i < 10; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
            DisposeRecords();
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, firstRecord.Id);
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, secoundRecord.Id);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(15)]
        [Test(Author = "Konrad Dysput", Description = "Test FIFO list retry order")]
        public void TestFifoListOrder(int totalNumberOfRecords)
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Queue);
            var records = new List<BacktraceDatabaseRecord>();
            for (int i = 0; i < totalNumberOfRecords; i++)
            {
                var record = GetRecord();
                records.Add(record);
                var dbRecord = _database.BacktraceDatabaseContext.Add(record);
            }
            DisposeRecords();
            foreach (var record in records)
            {
                var firstRecordId = _database.BacktraceDatabaseContext.FirstOrDefault().Id;
                Assert.AreEqual(firstRecordId, record.Id);
            }
        }

        [Test(Author = "Konrad Dysput", Description = "Test LIFO retry order")]
        public void TestLifoOrder()
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Stack);
            for (int i = 0; i < 10; i++)
            {
                _database.BacktraceDatabaseContext.Add(GetRecord());
            }
            //mock two records
            var firstRecord = GetRecord();
            var secoundsRecord = GetRecord();
            //test if first record is a real first record
            _database.BacktraceDatabaseContext.Add(firstRecord);
            //test received record after first pop
            _database.BacktraceDatabaseContext.Add(secoundsRecord);

            DisposeRecords();
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, secoundsRecord.Id);
            Assert.AreEqual(_database.BacktraceDatabaseContext.FirstOrDefault().Id, firstRecord.Id);
        }

        [TestCase(1)]
        [TestCase(5)]
        [TestCase(15)]
        [Test(Author = "Konrad Dysput", Description = "Test LIFO list retry order")]
        public void TestLifoListOrder(int totalNumberOfRecords)
        {
            _database.Clear();
            ChangeRetryOrder(RetryOrder.Stack);
            var records = new List<BacktraceDatabaseRecord>();
            for (int i = 0; i < totalNumberOfRecords; i++)
            {
                var record = GetRecord();
                records.Add(record);
                _database.BacktraceDatabaseContext.Add(record);
            }
            DisposeRecords();
            for (int retryIndex = records.Count - 1; retryIndex >= 0; retryIndex--)
            {
                var record = records[retryIndex];
                var firstRecordId = _database.BacktraceDatabaseContext.FirstOrDefault().Id;
                Assert.AreEqual(firstRecordId, record.Id);
            }
        }


    }
}

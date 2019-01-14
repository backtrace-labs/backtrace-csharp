using Backtrace.Extensions;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Backtrace.Tests.DeduplicationTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Conditions")]
    internal class BacktraceContextWithDeduplicationTests
    {
        /// <summary>
        /// Backtrace database instance for testing purpose
        /// </summary>
        private BacktraceDatabase _backtraceDatabase;

        /// <summary>
        /// Current project directory - any database path
        /// </summary>
        private readonly string _projectDirectory = Path.GetTempPath();

        private readonly uint _batchLimit = 3;

        [SetUp]
        public void Setup()
        {
            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.GetRecords())
                .Returns(new List<FileInfo>());
            mockFileContext.Setup(n => n.Clear());

            ////mock api
            var mockApi = new Mock<IBacktraceApi>();
            mockApi.Setup(n => n.Send(It.IsAny<BacktraceData>()))
                .Returns(BacktraceResult.OnError(It.IsAny<BacktraceReport>(), new Exception("error")));

            var backtraceDatabaseSettings = new BacktraceDatabaseSettings(_projectDirectory)
            {
                RetryLimit = _batchLimit,
                RetryBehavior = Types.RetryBehavior.NoRetry,
                AutoSendMode = false,
                DeduplicationStrategy = DeduplicationStrategy.Application | DeduplicationStrategy.Classifier | DeduplicationStrategy.Message
            };

            _backtraceDatabase = new BacktraceDatabase(backtraceDatabaseSettings)
            {
                BacktraceApi = mockApi.Object,
                BacktraceDatabaseFileContext = mockFileContext.Object
            };
            _backtraceDatabase.Start();
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test deduplication with database context - state after Add method")]
        public void TestCounter_AfterAddMethod_ShouldReturnValidNumberOfRecords(int numberOfDeduplications)
        {
            _backtraceDatabase.Clear();
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            for (int i = 0; i < numberOfDeduplications; i++)
            {
                _backtraceDatabase.Add(backtraceReport, null);
            }
            Assert.AreEqual(numberOfDeduplications, _backtraceDatabase.Count());
            var record = _backtraceDatabase.Get().FirstOrDefault();
            Assert.AreEqual(record.Count, numberOfDeduplications);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test deduplication with database context - state after Batch move")]
        public void TestCounter_AfterBuchMove_ShouldReturnValidNumberOfRecords(int numberOfRecords)
        {
            _backtraceDatabase.Clear();
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            for (int i = 0; i < numberOfRecords; i++)
            {
                _backtraceDatabase.Add(backtraceReport, null);
            }
            for (int i = 0; i < _batchLimit - 1; i++)
            {
                _backtraceDatabase.BacktraceDatabaseContext.IncrementBatchRetry();
            }
            Assert.AreEqual(numberOfRecords, _backtraceDatabase.Count());
            _backtraceDatabase.BacktraceDatabaseContext.IncrementBatchRetry();
            var record = _backtraceDatabase.Get()?.FirstOrDefault();
            Assert.IsNull(record);
            Assert.AreEqual(0, _backtraceDatabase.Count());
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test deduplication with database context - state after Flush sync")]
        public void TestCounter_AfterSyncFlushMethod_ShouldReturnZeroRecords(int numberOfRecords)
        {
            _backtraceDatabase.Clear();
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            for (int i = 0; i < numberOfRecords; i++)
            {
                _backtraceDatabase.Add(backtraceReport, null).Dispose();
            }
            Assert.AreEqual(numberOfRecords, _backtraceDatabase.Count());
            _backtraceDatabase.Flush();
            Assert.AreEqual(0, _backtraceDatabase.Count());
            var record = _backtraceDatabase.Get()?.FirstOrDefault();
            Assert.IsNull(record);
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(20)]
        [Test(Author = "Konrad Dysput", Description = "Test deduplication with database context - state after Flush async")]
        public async Task TestCounter_AfterAsyncFlushMethod_ShouldReturnZeroRecords(int numberOfRecords)
        {
            _backtraceDatabase.Clear();
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            for (int i = 0; i < numberOfRecords; i++)
            {
                //dispose record after add to let database know we don't use this record anymore
                _backtraceDatabase.Add(backtraceReport, null).Dispose();
            }
            Assert.AreEqual(numberOfRecords, _backtraceDatabase.Count());
            await _backtraceDatabase.FlushAsync();
            Assert.AreEqual(0, _backtraceDatabase.Count());
            var record = _backtraceDatabase.Get()?.FirstOrDefault();
            Assert.IsNull(record);
        }
    }
}

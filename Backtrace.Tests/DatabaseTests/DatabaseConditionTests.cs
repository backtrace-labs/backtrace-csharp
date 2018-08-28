using Backtrace.Extensions;
using Backtrace.Interfaces;
using Backtrace.Model.Database;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Conditions")]
    public class DatabaseConditionTests
    {
        /// <summary>
        /// Backtrace database instance for testing purpose
        /// </summary>
        private BacktraceDatabase _backtraceDatabase;

        /// <summary>
        /// Current project directory - any database path
        /// </summary>
        private readonly string _projectDirectory = Environment.CurrentDirectory;

        /// <summary>
        /// Total number of reports
        /// </summary>
        private readonly uint _totalNumberOfReports = 10;

        [SetUp]
        public void Setup()
        {
            //mock file context
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.GetRecords())
                .Returns(new List<FileInfo>());
            mockFileContext.Setup(n => n.Clear());

            var backtraceDatabaseSettings = new BacktraceDatabaseSettings(_projectDirectory)
            {
                MaxRecordCount = _totalNumberOfReports
            };

            _backtraceDatabase = new BacktraceDatabase(backtraceDatabaseSettings)
            {
                BacktraceDatabaseFileContext = mockFileContext.Object
            };
            //start database
            _backtraceDatabase.Start();
        }

        [Test(Author = "Konrad Dysput", Description = "Test database limit")]
        public void TestDatabaseRecordLimitConditions()
        {
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            _backtraceDatabase.Clear();
            for (int i = 0; i < _totalNumberOfReports; i++)
            {
                _backtraceDatabase.Add(backtraceReport, new Dictionary<string, object>(), Types.MiniDumpType.None);
            }
            Assert.Throws<ArgumentException>(() => _backtraceDatabase.Add(backtraceReport, new Dictionary<string, object>(),Types.MiniDumpType.None));
        }

        [TestCase(1)]
        [TestCase(2)]
        [Test(Author = "Konrad Dysput", Description = "Test database limit")]
        public void TestInvalidDatabaseRecordLimitConditions(int totalNumberOfRecords)
        {
            var backtraceReport = (new Exception("custom exception")).ToBacktraceReport();
            _backtraceDatabase.Clear();
            for (int i = 0; i < totalNumberOfRecords; i++)
            {
                Assert.DoesNotThrow(() => _backtraceDatabase.Add(backtraceReport, new Dictionary<string, object>(), Types.MiniDumpType.None));
            }
        }
    }
}

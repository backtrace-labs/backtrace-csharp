using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backtrace.Interfaces;
using Backtrace.Types;

namespace Backtrace.Tests.DatabaseTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Database")]
    public class DatabaseTests
    {
        private BacktraceDatabase<object> _database;

        [SetUp]
        public void Setup()
        {
            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Save(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()));
            var mockContext = new Mock<IBacktraceDatabaseContext<object>>();
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext<object>>();

            string projectDirectory = Environment.CurrentDirectory;
            _database = new BacktraceDatabase<object>(projectDirectory)
            {
                BacktraceDatabaseContext = mockContext.Object
            };
        }

        [Test(Author = "Konrad Dysput", Description = "Test database initialization")]
        public void TestDatabaseInitalizationConditions()
        {
            //initialize disabled database
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>());
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(string.Empty)));

            //initialize database with invalid arguments
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings("not existing directory")));
            string projectDirectory = Environment.CurrentDirectory;
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(projectDirectory) { TotalRetry = 0 }));

            //initialize database with valid settings
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(projectDirectory)));
        }

        [Test(Author = "Konrad Dysput", Description = "Test database add and delete methods")]
        public void TestReportFlow()
        {
            var testedReport = (new Exception("test exception")).ToBacktraceReport();
            var entry = _database.Add(testedReport, new Dictionary<string, object>(), MiniDumpType.None);
            _database.Delete(entry);
        }

        [Test(Author = "Konrad Dysput", Description = "Test database flush method")]
        public void TestFlushMethods()
        {
            var mockApi = new Mock<IBacktraceApi<object>>();
            mockApi.Setup(n => n.Send(It.IsAny<BacktraceData<object>>()))
                .Returns(new BacktraceResult());
            _database.SetApi(mockApi.Object);
            
            var testedReport = (new Exception("test exception")).ToBacktraceReport();
            for (int i = 0; i < 10; i++)
            {
                _database.Add(testedReport, new Dictionary<string, object>(), MiniDumpType.None);
            }
            var total = _database.Count();
            _database.Flush();
            Assert.AreNotEqual(total, _database.Count());
            Assert.AreEqual(0 , _database.Count());
        }

    }
}

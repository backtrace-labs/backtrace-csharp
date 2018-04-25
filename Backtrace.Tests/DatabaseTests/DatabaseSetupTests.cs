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
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Setup")]
    public class DatabaseSetupTests
    {
        private BacktraceDatabase<object> _database;
        private readonly string _projectDirectory = Environment.CurrentDirectory;

        [SetUp]
        public void Setup()
        {
            //mock single entry
            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Save(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()));

            //mock database context
            var mockContext = new Mock<IBacktraceDatabaseContext<object>>();

            //mock physical files operations
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext<object>>();
            mockFileContext.Setup(n => n.RemoveOrphaned(It.IsAny<IEnumerable<BacktraceDatabaseEntry<object>>>()));

            //initialize new database object
            _database = new BacktraceDatabase<object>(_projectDirectory)
            {
                BacktraceDatabaseContext = mockContext.Object,
                BacktraceDatabaseFileContext = mockFileContext.Object
            };
            //start database
            _database.Start();
        }

        [Test(Author = "Konrad Dysput", Description = "Test database initialization")]
        public void TestDatabaseInitalizationConditions()
        {
            //initialize disabled database
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>());
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(string.Empty)));

            //initialize database with invalid arguments
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings("not existing directory")));
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(_projectDirectory) { TotalRetry = 0 }));

            //initialize database with valid settings
            Assert.DoesNotThrow(() => new BacktraceDatabase<object>(new BacktraceDatabaseSettings(_projectDirectory)));
        }

        [Test(Author ="Konrad Dysput",Description ="Test not started database")]
        public void TestNotStartedDatabase()
        {
            var database = new BacktraceDatabase<object>(new BacktraceDatabaseSettings(_projectDirectory));
            var report = (new Exception("test excetpion")).ToBacktraceReport();
            Assert.DoesNotThrow(() => database.Add(null, null));
            Assert.AreEqual(null, database.Add(report, new Dictionary<string, object>()));
            Assert.DoesNotThrow(() => database.Clear());
            Assert.DoesNotThrow(() => database.Count());

            //mock deleting
            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Delete());
            Assert.DoesNotThrow(() => database.Delete(mockEntry.Object));
            Assert.DoesNotThrow(() => database.Delete(null));

            //test flush methods
            Assert.Throws<ArgumentException>(() => database.Flush());
            Assert.ThrowsAsync<ArgumentException>(() => database.FlushAsync());

            //mock api
            database.SetApi(new BacktraceApi<object>(new BacktraceCredentials("https://www.backtrace.io","123123")));
            Assert.DoesNotThrow(() => database.Flush());
            Assert.DoesNotThrowAsync(() => database.FlushAsync());
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
            Assert.AreEqual(0, _database.Count());
        }

    }
}

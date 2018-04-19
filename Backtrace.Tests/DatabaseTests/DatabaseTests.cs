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

namespace Backtrace.Tests.DatabaseTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Database")]
    public class DatabaseTests
    {
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

            var mockEntry = new Mock<BacktraceDatabaseEntry<object>>();
            mockEntry.Setup(n => n.Save(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()));

            var database = new BacktraceDatabase<object>();
            var entry = database.Add(testedReport, new Dictionary<string, object>());
            database.Delete(entry);
        }

    }
}

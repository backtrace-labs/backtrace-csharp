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
        /// <summary>
        /// Current project directory
        /// </summary>
        private readonly string _projectDirectory = System.IO.Path.GetTempPath();

        [Test(Author = "Konrad Dysput", Description = "Test database initialization")]
        public void TestDatabaseInitalizationConditions()
        {
            //initialize disabled database
            Assert.DoesNotThrow(() =>
            {
                var database = new BacktraceDatabase();
                database.Start();
            });
            Assert.DoesNotThrow(() =>
            {
                var database = new BacktraceDatabase(new BacktraceDatabaseSettings(string.Empty));
                database.Start();
            });

            //initialize database with invalid arguments
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase(new BacktraceDatabaseSettings("not existing directory")));
            Assert.Throws<ArgumentException>(() => new BacktraceDatabase(new BacktraceDatabaseSettings(_projectDirectory) { RetryLimit = 0 }));

            //initialize database with valid settings
            Assert.DoesNotThrow(() => new BacktraceDatabase(new BacktraceDatabaseSettings(_projectDirectory)));
        }

        [Test(Author = "Konrad Dysput", Description = "Test uninitialized database")]
        public void TestUninitializedDatabase()
        {
            var mockFileContext = new Mock<IBacktraceDatabaseFileContext>();
            mockFileContext.Setup(n => n.Clear());
            var database = new BacktraceDatabase(new BacktraceDatabaseSettings(_projectDirectory))
            {
                BacktraceDatabaseFileContext = mockFileContext.Object
            };
            var report = (new Exception("test excetpion")).ToBacktraceReport();
            Assert.DoesNotThrow(() => database.Add(null, null));
            Assert.AreEqual(null, database.Add(report, new Dictionary<string, object>()));
            Assert.DoesNotThrow(() => database.Clear());
            Assert.DoesNotThrow(() => database.Count());

            //mock deleting
            var mockRecord = new Mock<BacktraceDatabaseRecord>();
            mockRecord.Setup(n => n.Delete());
            Assert.DoesNotThrow(() => database.Delete(mockRecord.Object));
            Assert.DoesNotThrow(() => database.Delete(null));

            //test flush methods
            Assert.Throws<ArgumentException>(() => database.Flush());
            Assert.ThrowsAsync<ArgumentException>(() => database.FlushAsync());

            //mock api
            database.SetApi(new BacktraceApi(new BacktraceCredentials("https://www.backtrace.io", "123123")));
            Assert.DoesNotThrow(() => database.Flush());
            Assert.DoesNotThrowAsync(() => database.FlushAsync());
        }

    }
}

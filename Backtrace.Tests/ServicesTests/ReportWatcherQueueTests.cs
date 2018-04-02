using Backtrace.Model;
using Backtrace.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backtrace.Tests.ServicesTests
{
    /// <summary>
    /// Test a ReportWatcher class. Cless Queue behaviour and initialization. 
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Services.Watcher", Description = "Test queue report watcher behaviour")]
    public class ReportWatcherQueueTests
    {
        /// <summary>
        /// Test queue add and dequeue operation in report watcher
        /// </summary>
        [Test(Author = "Konrad Dysput", Description = "Test queue add operation")]
        public void TestQueueAddOperations()
        {
            var reportWatcher = new ReportWatcher<object>(2);
            Assert.DoesNotThrow(() => reportWatcher.WatchReport(new BacktraceReport("text information")));
            Thread.Sleep(3000);
            Assert.IsTrue(reportWatcher.WatchReport(new BacktraceReport("last available information")));
            Assert.IsFalse(reportWatcher.WatchReport(new BacktraceReport("invalid message")));
            reportWatcher.Reset();
            Assert.IsTrue(reportWatcher.WatchReport(new BacktraceReport("after clean ")));
            Assert.IsTrue(reportWatcher._reportQue.Count == 1);
        }

        /// <summary>
        /// Test watcher initialization and typical behavior for specific watcher configuration
        /// </summary>
        [Test(Author = "Konrad Dysput", Description = "Check queue add operation")]
        public void TestWatcherInitialization()
        {
            //watcher not work
            var reportWatcher = new ReportWatcher<object>(0);
            uint max = uint.MaxValue;
            Assert.Throws<OverflowException>(() => new ReportWatcher<object>(max *2));
            Assert.True(reportWatcher.WatchReport(new BacktraceReport("test")));
        }
    }
}

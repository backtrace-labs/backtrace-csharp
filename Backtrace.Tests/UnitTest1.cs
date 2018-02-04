using Backtrace.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Backtrace.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var watcher = new BackgroundWatcher(3);
            Assert.ThrowsException<NotImplementedException>(() => watcher.Initialize());

        }
    }
}

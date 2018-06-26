using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests.Model
{
    /// <summary>
    /// Class created for testing purpose
    /// </summary>
    internal class MockBacktraceDatabaseContext : BacktraceDatabaseContext
    {
        public MockBacktraceDatabaseContext(string path, uint retryNumber, RetryOrder retryOrder)
            : base(path, retryNumber, retryOrder)
        { }

        public void DisposeUsedFiles()
        {
            Get().ToList().ForEach(n => n.Locked = false);
        }
    }
}

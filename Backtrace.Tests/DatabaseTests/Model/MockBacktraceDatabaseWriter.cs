using Backtrace.Interfaces.Database;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests.Model
{
    internal class MockBacktraceDatabaseWriter : BacktraceDatabaseEntryWriter, IBacktraceDatabaseEntryWriter
    {
        public bool writeFail = false;

        public MockBacktraceDatabaseWriter()
            : base(string.Empty)
        { }
        
        public override void SaveTemporaryFile(string path, byte[] file)
        {
            if (writeFail)
            {
                throw new UnauthorizedAccessException();
            }
        }

        public override string ToJsonFile(object data)
        {
            return string.Empty;
        }

        public override void SaveValidReport(string sourcePath, string destinationPath)
        {
            return;
        }
    }
}

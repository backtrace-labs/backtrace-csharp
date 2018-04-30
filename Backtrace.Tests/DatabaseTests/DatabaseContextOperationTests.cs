using Backtrace.Interfaces;
using Backtrace.Model.Database;
using Backtrace.Tests.DatabaseTests.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    /// <summary>
    /// Test cache operations
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Database.Context")]
    public class DatabaseContextOperationTests : DatabaseTestBase
    {
        [Test(Author = "Konrad Dysput", Description = "Test add operation")]
        public void TestNewContextEntries()
        {
            throw new NotImplementedException();
        }
    }
}

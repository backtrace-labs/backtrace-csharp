using Backtrace.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.DatabaseTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "EnvironmentTests.Attributes")]
    public class DatabaseTests
    {

        [Test(Author = "Konrad Dysput", Description = "Test invalid initialization parameters")]
        public void TestDatabaseInitalizationConditions()
        {
            //Assert.Throws(() => new BacktraceDatabase<object>())
        }
    }
}

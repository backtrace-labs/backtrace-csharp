using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Backtrace.Base;
using Backtrace.Model;
using Backtrace.Model.JsonData;
using NUnit.Framework;
namespace Backtrace.Tests.EnvironmentTests
{
    /// <summary>
    /// Test built in attributes creation
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "EnvironmentTests.Attributes")]
    public class BacktraceAttributesTests
    {
        [Test]
        public void TestAttributesCreation()
        {
            var report = new BacktraceReportBase<string>("testMessage");
            //test object creation
            Assert.DoesNotThrow(() => new BacktraceAttributes<string>(report, null));

            //test empty exception
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes<string>(report,new Dictionary<string, string>());
                backtraceAttributes.SetExceptionAttributes(new Exception());
            });
            //test null
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes<string>(report,new Dictionary<string, string>());
                backtraceAttributes.SetExceptionAttributes(null);
            });
        }
    }
}

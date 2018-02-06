using System;
using System.Collections.Generic;
using System.Text;
using Backtrace.Model;
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
            //test object creation
            Assert.DoesNotThrow(() => new BacktraceAttributes());

            //test empty exception
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes();
                backtraceAttributes.SetExceptionAttributes(new Exception());
            });
            //test null
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes();
                backtraceAttributes.SetExceptionAttributes(null);
            });
        }
    }
}

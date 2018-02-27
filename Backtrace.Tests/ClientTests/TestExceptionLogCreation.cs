using Backtrace.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.ClientTests
{
    /// <summary>
    /// Test message log generation from
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Client.MsgLogCreation", Description = "Test client messages")]
    public class TestExceptionLogCreation : LogCreationBase
    {
        private static IEnumerable<Exception> _exceptions = new List<Exception>()
            {
                null,
                new Exception("exception"),
                new InvalidOperationException("invalidOperation", new ArgumentException("argumentException"))
            };

        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }
        [Test]
        public void TestLogCreation([ValueSource("_exceptions")]Exception exception)
        {
            Assert.DoesNotThrow(() => _backtraceClient.Send(exception: exception));
            Assert.DoesNotThrow(() => _backtraceClient.Send(new BacktraceReport(exception)));
        }

        [Test]
        public void TestExceptionAttributes([ValueSource("_exceptions")]Exception exception)
        {
            Dictionary<string, string> currentAttributes = new Dictionary<string, string>();
            _backtraceClient.BeforeSend =
                (BacktraceData<object> model) =>
                {
                    currentAttributes = model.Attributes;
                };

            foreach (var testAttributes in _testAttributes)
            {
                _backtraceClient.Send(exception, testAttributes);
                Assert.IsTrue(currentAttributes.Count != 0);
                Assert.IsTrue(currentAttributes.ContainsKey("ScopedAttributes"));
                int testAttributeCount = testAttributes?.Count ?? 0;
                int totalAttributes = testAttributeCount + _backtraceClient.Attributes.Count;
                Assert.IsTrue(totalAttributes <= currentAttributes.Count);
            }
        }
    }
}

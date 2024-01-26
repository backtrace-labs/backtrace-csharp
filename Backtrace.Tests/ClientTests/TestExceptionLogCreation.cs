using Backtrace.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Backtrace.Tests.ClientTests
{
    /// <summary>
    /// Test message log generation from
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Client.MsgLogCreation", Description = "Test client messages")]
    public class TestExceptionLogCreation : LogCreationBase
    {
        private static readonly IEnumerable<Exception> _exceptions = new List<Exception>()
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
        public void TestLogCreation([ValueSource("_exceptions")] Exception exception)
        {
            Assert.DoesNotThrow(() => _backtraceClient.Send(exception: exception));
            Assert.DoesNotThrow(() => _backtraceClient.Send(message: "test message"));
            Assert.DoesNotThrow(() => _backtraceClient.Send(new BacktraceReport(exception)));
        }

        [Test]
        public void TestReportStackTrace_StackTraceShouldBeTheSameLikeExceptionStackTrace_ShouldReturnCorrectStackTrace()
        {
            var exception = new Exception("exception");
            var report = new BacktraceReport(exception);
            Assert.AreEqual(report.DiagnosticStack.Count, exception.StackTrace?.Count() ?? 0);
        }

        [Test]
        public void TestReportStackTrace_StackTraceShouldIncludeEnvironmentStackTrace_ShouldReturnCorrectStackTrace()
        {
            var environmentStackTrace = new StackTrace(true);
            var report = new BacktraceReport("msg");
            Assert.AreEqual(report.DiagnosticStack.Count, environmentStackTrace.FrameCount);
        }

        [Test]
        public void TestExceptionAttributes([ValueSource("_exceptions")] Exception exception)
        {
            Dictionary<string, object> currentAttributes = new Dictionary<string, object>();
            _backtraceClient.BeforeSend =
                (BacktraceData model) =>
                {
                    currentAttributes = model.Attributes;
                    return model;
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

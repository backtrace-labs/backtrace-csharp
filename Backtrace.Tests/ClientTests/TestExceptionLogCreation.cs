using Backtrace.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
        public void TestLogCreation([ValueSource("_exceptions")]Exception exception)
        {
            Assert.DoesNotThrow(() => _backtraceClient.Send(exception: exception));
            Assert.DoesNotThrow(() => _backtraceClient.Send(message: "test message"));
            Assert.DoesNotThrow(() => _backtraceClient.Send(new BacktraceReport(exception)));
        }

        [Test]
        public void TestEnvironmentStackTrace_EnvironmentStakcTraceIsEqualToTrue_ShouldReturnCorrectStackTrace()
        {
            //this type of exception return empty stack trace
            //in this test if exception is empty and we don't include environemnt stacktrace
            //diagnostic stack will be empty
            var report = new BacktraceReport(new Exception("exception"), includeEnvironmentStacktrace: true);
            Assert.IsTrue(report.DiagnosticStack?.Any());
        }
        [Test]
        public void TestEnvironmentStackTrace_DefaultStackTraceParameter_ShouldReturnCorrectStackTrace()
        {
            var report = new BacktraceReport(new Exception("exception"));
            Assert.IsTrue(report.DiagnosticStack?.Any());
        }

        [Test]
        public void TestEnvironmentStackTrace_EnvironmentStakcTraceIsEqualToFalse_ShouldReturnCorrectStackTrace()
        {
            var report = new BacktraceReport(new Exception("exception"), includeEnvironmentStacktrace: false);
            Assert.IsFalse(report.DiagnosticStack?.Any());
        }

        [Test]
        public void TestExceptionAttributes([ValueSource("_exceptions")]Exception exception)
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

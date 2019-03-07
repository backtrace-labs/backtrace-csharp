using Backtrace.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backtrace.Tests.ClientTests
{
    /// <summary>
    /// Test message log generation from
    /// </summary>
    [TestFixture(Author = "Konrad Dysput", Category = "Client.MsgLogCreation", Description = "Test client messages")]
    public class TestMessageLogCreation : LogCreationBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("message")]
        [TestCase("!@#$%^&*()_+}{\":?><ZXC")]
        [Test(Author = "Konrad Dysput", Description = "Test diffrent report message")]
        public void TestLogCreation(string message)
        {
            Assert.DoesNotThrow(() => _backtraceClient.Send(message: message));
            Assert.DoesNotThrow(() => _backtraceClient.Send(new BacktraceReport(message)));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("message")]
        [TestCase("!@#$%^&*()_+}{\":?><ZXC")]
        [Test(Author = "Konrad Dysput", Description = "Test messages with attributes")]
        public void TestMessageAttributes(string message)
        {
            var currentAttributes = new Dictionary<string, object>();
            _backtraceClient.BeforeSend =
                (BacktraceData model) =>
                {
                    currentAttributes = model.Attributes;
                    return model;
                };

            foreach (var testAttributes in _testAttributes)
            {
                _backtraceClient.Send(message, testAttributes);
                Assert.IsTrue(currentAttributes.Count != 0);
                Assert.IsTrue(currentAttributes.ContainsKey("ScopedAttributes"));
                int testAttributeCount = testAttributes?.Count ?? 0;
                int totalAttributes = testAttributeCount + _backtraceClient.Attributes.Count;
                Assert.IsTrue(totalAttributes <= currentAttributes.Count);
            }
        }

    }
}

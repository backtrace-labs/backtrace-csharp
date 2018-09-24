using Backtrace.Model;
using Backtrace.Model.JsonData;
using NUnit.Framework;
using System.Collections.Generic;
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
            var report = new BacktraceReport("testMessage");
            //test object creation
            Assert.DoesNotThrow(() => new BacktraceAttributes(report, null));

            //test empty exception
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes(report, new Dictionary<string, object>());
                backtraceAttributes.SetExceptionAttributes(new BacktraceReport("message"));
            });
            //test null
            Assert.DoesNotThrow(() =>
            {
                var backtraceAttributes = new BacktraceAttributes(report, new Dictionary<string, object>());
                backtraceAttributes.SetExceptionAttributes(null);
            });
        }


        [Test]
        public void TestFingerprintAttribute()
        {
            string fingerprint = "fingerprint for testing purpose";
            var report = new BacktraceReport("testMessage")
            {
                Fingerprint = fingerprint
            };
            var attributes = new BacktraceAttributes(report, null);
            Assert.IsTrue(attributes.Attributes.ContainsKey("_mod_fingerprint"));
            Assert.IsFalse(attributes.Attributes.ContainsKey("_mod_factor"));

            Assert.AreEqual(attributes.Attributes["_mod_fingerprint"], fingerprint);
        }

        [Test]
        public void TestFactorAttribute()
        {
            string factor = "factor attribute value";
            var report = new BacktraceReport("testMessage")
            {
                Factor = factor
            };

            var attributes = new BacktraceAttributes(report, null);
            Assert.IsTrue(attributes.Attributes.ContainsKey("_mod_factor"));

            Assert.AreEqual(attributes.Attributes["_mod_factor"], factor);
            Assert.IsFalse(attributes.Attributes.ContainsKey("_mod_fingerprint"));
        }
    }
}

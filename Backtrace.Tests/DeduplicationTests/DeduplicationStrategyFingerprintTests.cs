using Backtrace.Model;
using Backtrace.Types;
using NUnit.Framework;
using System;

namespace Backtrace.Tests.DeduplicationTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Deduplication.Fingerprint")]
    public class DeduplicationStrategyFingerPrintTests
    {
        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking empty stack trace")]
        public void TestShaComparision_Fingerprintavailable_DeduplicationReturnsFingerprint()
        {
            string fingerprint = "12345";
            var exception = new Exception("test message");
            var report = new BacktraceReport(exception)
            {
                Fingerprint = fingerprint
            };
            var data = new BacktraceData(report, null);
            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Default);
            var sha = deduplicationModel.GetSha();
            Assert.AreEqual(sha, fingerprint);
        }


        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with different options with data object")]
        [TestCase(DeduplicationStrategy.Message | DeduplicationStrategy.Classifier | DeduplicationStrategy.LibraryName)]
        [TestCase(DeduplicationStrategy.LibraryName | DeduplicationStrategy.Message)]
        public void TestShaComparisionWithMultipleOptionsAndReports_MultipleOptions_DeduplicationReturnsFingerprint(DeduplicationStrategy strategy)
        {
            var fingerprint = "12345";
            var exception = new Exception("testMessage");
            var report = new BacktraceReport(exception)
            {
                Fingerprint = fingerprint
            };

            var data = new BacktraceData(report, null);
            var deduplicationModel = new DeduplicationModel(data, strategy);
            var sha = deduplicationModel.GetSha();
            Assert.AreEqual(sha, fingerprint);
        }
    }
}

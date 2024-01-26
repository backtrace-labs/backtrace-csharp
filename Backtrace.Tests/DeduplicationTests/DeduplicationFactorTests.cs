using Backtrace.Model;
using Backtrace.Types;
using NUnit.Framework;
using System;

namespace Backtrace.Tests.DeduplicationTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Deduplication.Factor")]
    public class DeduplicationFactorTests
    {
        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking empty stack trace")]
        public void TestShaComparision_OnlyEmptyStackTrace_ReportWithFactorGenerateDifferentSha()
        {
            var report = new BacktraceReport(new Exception("test message"));
            var reportWithFactor = new BacktraceReport(new Exception("test message"))
            {
                Factor = "12345"
            };

            var data = new BacktraceData(report, null);
            var dataWithFactor = new BacktraceData(reportWithFactor, null);
            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Default);
            var comparer = new DeduplicationModel(dataWithFactor, DeduplicationStrategy.Default);
            Assert.AreNotEqual(deduplicationModel.GetSha(), comparer.GetSha());
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking empty stack trace")]
        public void TestShaComparision_OnlyEmptyStackTrace_ReportsWithTheSameFactorAreTheSame()
        {
            var reportWithFactor = new BacktraceReport(new Exception("test message"))
            {
                Factor = "12345"
            };
            var reportWithFactor2 = new BacktraceReport(new Exception("test message"))
            {
                Factor = "12345"
            };

            var dataWithFactor2 = new BacktraceData(reportWithFactor2, null);
            var dataWithFactor = new BacktraceData(reportWithFactor, null);
            var result = new DeduplicationModel(dataWithFactor2, DeduplicationStrategy.Default);
            var comparer = new DeduplicationModel(dataWithFactor, DeduplicationStrategy.Default);
            Assert.AreEqual(result.GetSha(), comparer.GetSha());
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with different options with data object")]
        [TestCase(DeduplicationStrategy.Message | DeduplicationStrategy.Classifier | DeduplicationStrategy.LibraryName)]
        [TestCase(DeduplicationStrategy.LibraryName | DeduplicationStrategy.Message)]
        public void TestShaComparisionWithMultipleOptionsAndReports_MultipleOptions_ValidSha(DeduplicationStrategy strategy)
        {
            var exception = new Exception("testMessage");
            var report = new BacktraceReport(exception);

            var data = report.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, strategy);
            var comparer = new DeduplicationModel(data, strategy);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }
    }
}

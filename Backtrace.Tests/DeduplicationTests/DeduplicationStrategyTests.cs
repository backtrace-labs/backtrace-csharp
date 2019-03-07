using Backtrace.Model;
using Backtrace.Types;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Backtrace.Tests.DeduplicationTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "Deduplication.StrategyTests")]
    public class DeduplicationStrategyTests
    {
        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking empty stack trace")]
        public void TestShaComparision_OnlyEmptyStackTrace_ValidSha()
        {
            var report = new BacktraceReport(new Exception("test message"))
            {
                DiagnosticStack = new List<BacktraceStackFrame>()
            };
            var data = new BacktraceData(report, null);
            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Default);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Default);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }


        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace")]
        public void TestShaComparision_OnlyStackTrace_ValidSha()
        {
            var report = new BacktraceReport(new Exception("test message"));
            var data = report.ToBacktraceData(null);
            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Default);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Default);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace and classifier")]
        public void TestShaComparision_ClassifierStrategy_ValidSha()
        {
            var exception = new Exception("testMessage", new Exception("inner exception test message"));
            var report = new BacktraceReport(exception);
            var data = report.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Classifier);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Classifier);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace and exception message")]
        public void TestShaComparision_ExceptionMessageStrategy_ValidSha()
        {
            var exception = new Exception("testMessage");
            var report = new BacktraceReport(exception);
            var data = report.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Message);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Message);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }


        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace and report message")]
        public void TestShaComparisionWithReportMessage_ExceptionMessageStrategy_ValidSha()
        {
            string expectedString = "test";
            var report = new BacktraceReport(expectedString);
            var data = report.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Message);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Message);

            var deduplicationSha = deduplicationModel.GetSha();

            Assert.AreEqual(deduplicationModel.ExceptionMessage, expectedString);
            Assert.AreEqual(comparer.ExceptionMessage, expectedString);

            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace and different exception message")]
        public void TestShaComparisionWithDifferentMessages_ExceptionMessageStrategy_InvalidSha()
        {
            var exception = new Exception("testMessage");
            var comparerException = new Exception("comparer test message");

            var report = new BacktraceReport(exception);
            var comparerReport = new BacktraceReport(comparerException);

            var data = report.ToBacktraceData(null);
            var comparerData = comparerReport.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Message);
            var comparer = new DeduplicationModel(comparerData, DeduplicationStrategy.Message);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreNotEqual(deduplicationSha, comparerSha);
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with data object by checking stack trace and application name")]
        public void TestShaComparisionWithLibName_ApplicationStrategy_ValidSha()
        {
            var exception = new Exception("testMessage");

            var report = new BacktraceReport(exception);

            var data = report.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, DeduplicationStrategy.Application);
            var comparer = new DeduplicationModel(data, DeduplicationStrategy.Application);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreEqual(deduplicationSha, comparerSha);
        }


        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with different options with data object")]
        [TestCase(DeduplicationStrategy.Message | DeduplicationStrategy.Classifier | DeduplicationStrategy.Application)]
        [TestCase(DeduplicationStrategy.Application | DeduplicationStrategy.Message)]
        public void TestShaComparisionWithMultipleOptionsAndReports_MultipleOptions_InvalidSha(DeduplicationStrategy strategy)
        {
            var exception = new Exception("testMessage");
            var comparerException = new Exception("comparer test message");

            var report = new BacktraceReport(exception);
            var comparerReport = new BacktraceReport(comparerException);

            var data = report.ToBacktraceData(null);
            var comparerData = comparerReport.ToBacktraceData(null);

            var deduplicationModel = new DeduplicationModel(data, strategy);
            var comparer = new DeduplicationModel(comparerData, strategy);

            var deduplicationSha = deduplicationModel.GetSha();
            var comparerSha = comparer.GetSha();
            Assert.IsNotEmpty(deduplicationSha);
            Assert.IsNotEmpty(comparerSha);

            Assert.AreNotEqual(deduplicationSha, comparerSha);
        }

        [Test(Author = "Konrad Dysput", Description = "Test sha comparision with different options with data object")]
        [TestCase(DeduplicationStrategy.Message | DeduplicationStrategy.Classifier | DeduplicationStrategy.Application)]
        [TestCase(DeduplicationStrategy.Application | DeduplicationStrategy.Message)]
        public void TestShaComparisionWithMultipleOptions_MultipleOptions_ValidSha(DeduplicationStrategy strategy)
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

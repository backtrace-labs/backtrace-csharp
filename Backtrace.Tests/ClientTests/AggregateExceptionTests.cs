using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Types;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Backtrace.Tests.ClientTests
{
    [TestFixture(
        Author = "Konrad Dysput",
        Category = "Client.AggreagateException",
        Description = "Test client bahaviour for handling aggreagate exceptions")]
    public class AggregateExceptionTests
    {
        protected BacktraceClient _backtraceClient;

        [SetUp]
        public virtual void Setup()
        {
            var api = new Mock<IBacktraceApi>();
            api.Setup(n => n.Send(It.IsAny<BacktraceData>()))
                .Returns(new BacktraceResult() { Status = BacktraceResultStatus.Ok });

            api.Setup(n => n.SendAsync(It.IsAny<BacktraceData>()))
              .ReturnsAsync(() => new BacktraceResult() { Status = BacktraceResultStatus.Ok });

            var credentials = new BacktraceCredentials(@"https://validurl.com/", "validToken");
            _backtraceClient = new BacktraceClient(credentials)
            {
                BacktraceApi = api.Object,
                UnpackAggregateExcetpion = true
            };
        }

        [Test(Description = "Test empty aggregate exception")]
        public void TestEmptyAggreagateException()
        {
            var aggregateException = new AggregateException("test exception");
            var result = _backtraceClient.Send(aggregateException);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Status, BacktraceResultStatus.Empty);
        }

        [Test(Description = "Test default scenario for aggregate exception")]
        public void TestAggreagateException()
        {
            var aggregateException = new AggregateException("test exception",
                new List<Exception>() {
                     new ArgumentException("argument exception"),
                    new InvalidOperationException("invalid operation exception"),
                    new FormatException("format exception"),
                });
            var result = _backtraceClient.Send(aggregateException);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Status, BacktraceResultStatus.Ok);

            int totalReports = 0;
            while (result != null)
            {
                totalReports++;
                result = result.InnerExceptionResult;
            }
            Assert.AreEqual(3, totalReports);

        }
    }
}

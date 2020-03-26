using Backtrace.Model;
using Backtrace.Services;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;

namespace Backtrace.Tests.IntegrationTests
{
    [TestFixture(Author = "Konrad Dysput", Category = "IntegrationTests.SerializationTests", Description = "Should handle specific serialization errors")]
    public class SerializationTests
    {
        /// <summary>
        /// Backtrace client
        /// </summary>
        private BacktraceClient _backtraceClient;

        /// <summary>
        /// Prepare basic setup of Backtrace client
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var credentials = new BacktraceCredentials("https://validurl.com/", "validToken");

            //mock api
            var serverUrl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(serverUrl)
                .Respond("application/json", "{'object' : 'aaa'}");
            var api = new BacktraceApi(credentials, 0)
            {
                HttpClient = mockHttp.ToHttpClient()
            };

            //setup new client
            _backtraceClient = new BacktraceClient(credentials)
            {
                BacktraceApi = api
            };
        }

        /// <summary>
        /// Test Backtrace report serialization with self referenced objects in annotations
        /// </summary>
        [Test(Author = "Konrad Dysput", Description = "Test self referenced exception")]
        public void TestReportContainerSerialization_ExceptionReferenceLoop_ShouldIgnoreReferenceLoopInSerialization()
        {
            var error = new Exception("test error");

            var foo = new SelfReferencedClass()
            {
                ClassId = "foo1"
            };
            var foo2 = new SelfReferencedClass() { ClassId = "foo2", Next = foo };
            foo.Next = foo2;

            var report = new BacktraceReport(error, new Dictionary<string, object>() { { "annotation", foo } });
            Assert.DoesNotThrow(() => _backtraceClient.Send(report));
        }


        private class SelfReferencedClass
        {
            public SelfReferencedClass Next { get; set; }
            public string ClassId { get; set; }
        }

    }
}

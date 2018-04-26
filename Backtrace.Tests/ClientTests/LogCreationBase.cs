using Backtrace.Interfaces;
using Backtrace.Model;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Tests.ClientTests
{
    public class LogCreationBase
    {
        protected BacktraceClient _backtraceClient;
        protected List<Dictionary<string, object>> _testAttributes = new List<Dictionary<string, object>>();

        [SetUp]
        public virtual void Setup()
        {
            var api = new Mock<IBacktraceApi<object>>();
            api.Setup(n => n.Send(It.IsAny<Model.BacktraceData<object>>())).Returns(new Model.BacktraceResult());
            var credentials = new BacktraceCredentials(@"https://validurl.com/", "validToken");
            _backtraceClient = new BacktraceClient(credentials)
            {
                BacktraceApi = api.Object
            };

            //set one scoped attribute
            _backtraceClient.Attributes["ScopedAttributes"] = true;

            _testAttributes.Add(new Dictionary<string, object>()
            {
                {"boolAttribute", true },
                {"numberAttribute", 123 },
                {"stringAttribute", "attribute" },
                {"objectAttribute", new { Name = "Konrad", WorkType ="UnitTest"} }
            });

            _testAttributes.Add(new Dictionary<string, object>());
            _testAttributes.Add(null);
        }

    }
}

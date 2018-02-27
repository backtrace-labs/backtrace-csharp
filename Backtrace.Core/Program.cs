using Backtrace.Interfaces;
using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Backtrace.Core
{
    class Program
    {
        private static BacktraceClient backtraceClient = new BacktraceClient();
        private static void DoSomething(int i = 0)
        {
            if (i == 2)
            {
                throw new ArgumentException("i");
            }
            Thread.Sleep(20);
            try
            {
                DoSomething(++i);

            }
            catch (Exception e)
            {
                backtraceClient.Send(e);
            }
        }
        static void Main(string[] args)
        {
            //initialize new BacktraceClient with custom configuration section readed from file App.config
            //Client will be initialized with values stored in default section name "BacktraceCredentials"
            //var backtraceClient = new BacktraceClient();

            var credentials = new BacktraceCredentials("https://yourHostUrl.com", "accessToken");
            var backtraceClientWithCredentials = new BacktraceClient(credentials);

            //Add new scoped attributes
            backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "Backtrace",
                Type = "Library"
            };

            //Add your own handler to client API
            backtraceClient.BeforeSend =
               (Model.BacktraceData<object> model) =>
               {
                   var data = model;
               };

            //Report a new exception from current application
            try
            {
                Thread thread = new Thread(new ThreadStart(() => { DoSomething(0); }));
                thread.Start();
                thread.Join();
                var i = 0;
                var result = i / i;
            }
            catch (Exception exception)
            {
                var report = new BacktraceReport<object>(
                    exception: exception,
                    attributes: new Dictionary<string, object>() { { "AttributeString", "string" } },
                    attachmentPaths: new List<string>() { @"path to file attachment", @"patch to another file attachment" }
                );
                backtraceClient.Send(report);
            }

            //Report a new message
            backtraceClient.Send("Client message");
        }
    }
}

using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtrace.Framework45Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize new BacktraceClient with custom configuration section readed from file App.config
            //Client will be initialized with values stored in default section name "BacktraceCredentials"
            BacktraceClient backtraceClient = new BacktraceClient();

            var credentials = new BacktraceCredentials(@"https://myserver.sp.backtrace.io:6097", "4dca18e8769d0f5d10db0d1b665e64b3d716f76bf182fbcdad5d1d8070c12db0");
            var backtraceClientWithCredentials = new BacktraceClient(credentials);

            //Add new scoped attributes
            backtraceClient.Attributes["ClientAttributeNumber"] = 1;
            backtraceClient.Attributes["ClientAttributeString"] = "string attribute";
            backtraceClient.Attributes["ClientAttributeCustomClass"] = new
            {
                Name = "Backtrace",
                Type = "Library"
            };
            backtraceClient.OnServerResponse = (BacktraceServerResponse response) =>
            {
                System.Diagnostics.Trace.WriteLine(response.Object);
            };
            backtraceClient.OnServerError = (Exception e) =>
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            };
            //Add your own handler to client API
            backtraceClient.BeforeSend =
               (BacktraceData<object> model) =>
               {
                   var data = model;
                   data.Attributes.Add("eventAtrtibute", "EventAttributeValue");
                   return data;
               };

            //Report a new exception from current application
            try
            {
                try
                {
                    int.Parse("abc");
                }
                catch (Exception inner)
                {
                    try
                    {
                        var openLog = File.Open("Not existing path", FileMode.Open);
                    }
                    catch
                    {
                        throw new FileNotFoundException("OutterException", inner);
                    }
                }
            }
            catch (Exception e)
            {
                var report = new BacktraceReport(
                exception: e,
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

using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Backtrace.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize new BacktraceClient with custom configuration section readed from file App.config
            //Client will be initialized with values stored in default section name "BacktraceCredentials"
            BacktraceClient backtraceClient = new BacktraceClient();

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
                        var openLog = File.Open("DoesNotExist", FileMode.Open);
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

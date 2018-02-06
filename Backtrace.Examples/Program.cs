using System;
namespace Backtrace.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //initialize new BacktraceClient with custom configuration section readed from file App.config
            //Client will be initialized with values stored in default section name "BacktraceCredentials"
            var backtraceClient = new BacktraceClient();

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

            //Report a new message
            backtraceClient.Send("Client message");

            //Report a new exception from current application
            try
            {
                var i = 0;
                var result = i / i;
            }
            catch (Exception exception)
            {
                backtraceClient.Send(exception);
            }
        }
    }
}

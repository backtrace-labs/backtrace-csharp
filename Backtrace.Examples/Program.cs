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

            var credentials = new BacktraceCredentials("backtraceHostUrl", "accessToken");
            var backtraceClientWithCredentials = new BacktraceClient(credentials);

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

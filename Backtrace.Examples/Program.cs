using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}

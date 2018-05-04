using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backtrace.WinFoms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //initialize new BacktraceClient 
            BacktraceClient backtraceClient = new BacktraceClient(
                    new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token),
                    reportPerMin: 0 //unlimited number of reports per secound
            );
            //Setting application exceptions
            backtraceClient.HandleApplicationException();
            //sending custom client message to Backtrace server
            var result = backtraceClient.SendAsync("WPF Application crash report started").Result;
            if(result.Status == Types.BacktraceResultStatus.Ok)
            {
                Trace.WriteLine($"Report is availble on Backtrace API!");
            }
            
            Application.EnableVisualStyles();
            Application.ThreadException += backtraceClient.HandleApplicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}

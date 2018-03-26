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
            BacktraceClient backtraceClient = new BacktraceClient(
                    new BacktraceCredentials(ApplicationCredentials.Host, ApplicationCredentials.Token),
                    reportPerMin: 0 //unlimited number of reports per secound
            );
            backtraceClient.OnUnhandledApplicationException += (Exception e) =>
            {
                Trace.WriteLine(e.Message);
            };
            backtraceClient.HandleApplicationException();
            Application.EnableVisualStyles();
            Application.ThreadException += backtraceClient.HandleApplicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}

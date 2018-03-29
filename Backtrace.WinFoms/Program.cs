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
                    reportPerMin: 0, //unlimited number of reports per secound
                    tlsSupport: true
            );
            backtraceClient.OnServerError += (Exception e) =>
            {
                Trace.WriteLine(e.Message);
            };
            backtraceClient.HandleApplicationException();
            backtraceClient.SendAsync("WPF Application crash report started").Wait();
            Application.EnableVisualStyles();
            Application.ThreadException += backtraceClient.HandleApplicationThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());
        }
    }
}

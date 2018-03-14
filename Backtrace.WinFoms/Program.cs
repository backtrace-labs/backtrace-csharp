using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backtrace.WinFoms
{
    static class Program
    {
        static public void ExceptionFunction()
        {
            throw new Exception("Win forms uncaugh exception");
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            BacktraceClient backtraceClient = new BacktraceClient(
                    new BacktraceCredentials(@"http://yolo.sp.backtrace.io:6097/", "328174ab5c377e2cdcb6c763ec2bbdf1f9aa5282c1f6bede693efe06a479db54"),
                    reportPerMin: 0 //unlimited number of reports per secound
            );
            backtraceClient.HandleApplicationException();
            Application.EnableVisualStyles();
            //Application.ThreadException += backtraceClient.HandleApplicationThreadException;
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.SetCompatibleTextRenderingDefault(false);
          
            Application.Run(new Form1());
        }      
    }
}

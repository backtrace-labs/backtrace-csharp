using Backtrace.Model;
using Backtrace.Model.Database;
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
        /// Credentials
        /// </summary>
        private static BacktraceCredentials credentials = new BacktraceCredentials(ApplicationSettings.Host, ApplicationSettings.Token);

        /// <summary>
        /// Database settings
        /// </summary>
        private static BacktraceDatabaseSettings databaseSettings = new BacktraceDatabaseSettings(ApplicationSettings.DatabasePath);

        /// <summary>
        /// New instance of BacktraceClient. Check SetupBacktraceLibrary method for intiailization example
        /// </summary>
        private static BacktraceClient backtraceClient;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //create Backtrace library configuration
            var configuartion = new BacktraceClientConfiguration(credentials)
            {
                ReportPerMin = 0
            };
            //create Backtrace -
            var database = new BacktraceDatabase<object>(databaseSettings);
            //initialize new BacktraceClient 
            backtraceClient = new BacktraceClient(configuartion, database);
            //Setting application exceptions
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

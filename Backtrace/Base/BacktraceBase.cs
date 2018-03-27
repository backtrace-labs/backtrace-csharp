using Backtrace.Model;
using Backtrace.Interfaces;
using System;
using System.Collections.Generic;
using Backtrace.Services;
using Backtrace.Types;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Base
{
    /// <summary>
    /// Base Backtrace .NET Client 
    /// </summary>
    public class BacktraceBase<T>
    {

        /// <summary>
        /// Custom request handler for HTTP call to server
        /// </summary>
        public Func<string, string, BacktraceData<T>, BacktraceServerResponse> RequestHandler
        {
            get
            {
                return _backtraceApi.RequestHandler;
            }
            set
            {
                _backtraceApi.RequestHandler = value;
            }
        }

        /// <summary>
        /// Use asynchronous method to send report to server
        /// </summary>
        [Obsolete]
        public bool AsyncRequest
        {
            get
            {
                return _backtraceApi.AsynchronousRequest;
            }
            set
            {
                _backtraceApi.AsynchronousRequest = value;
            }
        }

        /// <summary>
        /// Set an event executed when received bad request, unauthorize request or other information from server
        /// </summary>
        public Action<Exception> OnServerError
        {
            get
            {
                return _backtraceApi.OnServerError;
            }
            set
            {
                _backtraceApi.OnServerError = value;
            }
        }

        /// <summary>
        /// Set an event executed when Backtrace API return information about send report
        /// </summary>
        public Action<BacktraceServerResponse> OnServerResponse
        {
            get
            {
                return _backtraceApi.OnServerResponse;
            }
            set
            {
                _backtraceApi.OnServerResponse = value;
            }
        }
        
        /// <summary>
        /// Get or set minidump type
        /// </summary>
        public MiniDumpType MiniDumpType { get; set; } = MiniDumpType.Normal;

        /// <summary>
        /// Set event executed when client site report limit reached
        /// </summary>
        public Action OnClientReportLimitReached = null;

        /// <summary>
        /// Set event executed before sending data to Backtrace API
        /// </summary>
        public Func<BacktraceData<T>, BacktraceData<T>> BeforeSend = null;

        /// <summary>
        /// Set event executed when unhandled application exception event catch exception
        /// </summary>
        public Action<Exception> OnUnhandledApplicationException = null;

        /// <summary>
        /// Get custom client attributes. Every argument stored in dictionary will be send to Backtrace API
        /// </summary>
        public Dictionary<string, T> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Client attributes
        /// </summary>
        protected Dictionary<string, T> _attributes;


        /// <summary>
        /// Instance of BacktraceApi that allows to send data to Backtrace API
        /// </summary>
        internal IBacktraceApi<T> _backtraceApi;

        /// <summary>
        /// Backtrace database instance that allows to manage minidump files 
        /// </summary>
        internal IBacktraceDatabase<T> _database;

        /// <summary>
        /// Backtrace report watcher that controls number of request sending per minute
        /// </summary>
        internal ReportWatcher<T> _reportWatcher;


        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        /// <param name="tlsSupport"></param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3,
            bool tlsSupport = false)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase<T>(databaseDirectory);
            _backtraceApi = new BacktraceApi<T>(backtraceCredentials);
            _reportWatcher = new ReportWatcher<T>(reportPerMin);
            if (tlsSupport)
            {
                _backtraceApi.SetTlsSupport();
            }
        }

        /// <summary>
        /// Change maximum number of reportrs sending per one minute
        /// </summary>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public void ChangeRateLimiting(uint reportPerMin)
        {
            _reportWatcher.ChangeRateLimiting(reportPerMin);
        }

        /// <summary>
        /// Send a report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual BacktraceServerResponse Send(BacktraceReportBase<T> report)
        {
            //get diagnostic data about current report and current application state
            var data = GetDiagnosticData(report);

            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = _backtraceApi.Send(data);

            //clear minidumps generated by app
            _database.ClearMiniDump(report.MinidumpFile);
            return result;
        }
#if !NET35
        /// <summary>
        /// Send asynchronous report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual async Task<BacktraceServerResponse> SendAsync(BacktraceReportBase<T> report)
        {
            //get diagnostic data about current report and current application state
            var data = GetDiagnosticData(report);

            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = await _backtraceApi.SendAsync(data);
            //clear minidumps generated by app
            _database.ClearMiniDump(report.MinidumpFile);
            return result;
        }

        /// <summary>
        /// Add automatic exception handling for current application
        /// </summary>
        public virtual void HandleApplicationException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Add automatic thread exception handling for current application
        /// </summary>
        public virtual void HandleApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            Send(new BacktraceReportBase<T>(e.Exception, assembly));
            OnUnhandledApplicationException?.Invoke(e.Exception);
        }

        /// <summary>
        /// In most situation when application crash, main process wont wait till we prepare report and send it to API. 
        /// Method allows you to get all necessary data required by BacktraceClient and wait till report will be send on server
        /// This method is invoked when application crash, so BacktraceClient override all existing events to make sure 
        /// we can handle request end
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            var exception = e.ExceptionObject as Exception; 
            var result = Send(new BacktraceReportBase<T>(exception, assembly));
        }
#endif

        /// <summary>
        /// Prepare diagnostic data about current application state
        /// </summary>
        /// <param name="report">Current report</param>
        /// <returns>Diagnostic data</returns>
        private BacktraceData<T> GetDiagnosticData(BacktraceReportBase<T> report)
        {
            //check rate limiting
            bool watcherValidation = _reportWatcher.WatchReport(report);
            if (!watcherValidation)
            {
                OnClientReportLimitReached?.Invoke();
                throw new ArgumentOutOfRangeException("Rate limiting reached");
            }
            //generate minidump and add minidump to report 
            string minidumpPath = _database.GenerateMiniDump(report, MiniDumpType);
            if (!string.IsNullOrEmpty(minidumpPath))
            {
                report.SetMinidumpPath(minidumpPath);
            }
            //create a JSON payload instance
            return new BacktraceData<T>(report, Attributes);
        }
    }
}

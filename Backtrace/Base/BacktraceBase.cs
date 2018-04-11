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
        public Func<string, string, BacktraceData<T>, BacktraceResult> RequestHandler
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
        public Action<BacktraceResult> OnServerResponse
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
        public Action<BacktraceReport> OnClientReportLimitReached = null;

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
        /// Backtrace database instance that allows to manage minidump files 
        /// </summary>
        public IBacktraceDatabase<T> Database;

        /// <summary>
        /// Client attributes
        /// </summary>
        protected Dictionary<string, T> _attributes;


        /// <summary>
        /// Instance of BacktraceApi that allows to send data to Backtrace API
        /// </summary>
        internal IBacktraceApi<T> _backtraceApi;


        /// <summary>
        /// Backtrace report watcher that controls number of request sending per minute
        /// </summary>
        internal ReportWatcher<T> _reportWatcher;

        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            BacktraceDatabaseSettings databaseSettings = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            Database = new BacktraceDatabase<T>(databaseSettings, backtraceCredentials);
            //_backtraceApi = new BacktraceApi<T>(backtraceCredentials, tlsLegacySupport);
            _reportWatcher = new ReportWatcher<T>(reportPerMin);
        }

        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            IBacktraceDatabase<T> database= null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            Database = database;
            //_backtraceApi = new BacktraceApi<T>(backtraceCredentials, tlsLegacySupport);
            _reportWatcher = new ReportWatcher<T>(reportPerMin);
        }

        /// <summary>
        /// Change maximum number of reportrs sending per one minute
        /// </summary>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        [Obsolete("This method has been deprecated, please use SetClientReportLimit instead.")]
        public void ChangeRateLimiting(uint reportPerMin)
        {
            _reportWatcher.SetClientReportLimit(reportPerMin);
        }

        /// <summary>
        /// Change maximum number of reportrs sending per one minute
        /// </summary>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public void SetClientReportLimit(uint reportPerMin)
        {
            _reportWatcher.SetClientReportLimit(reportPerMin);
        }


        /// <summary>
        /// Send a report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual BacktraceResult Send(BacktraceReportBase<T> report)
        {
            //check rate limiting
            bool watcherValidation = _reportWatcher.WatchReport(report);
            if (!watcherValidation)
            {
                var resultReport = report as BacktraceReport;
                OnClientReportLimitReached?.Invoke(resultReport);
                return BacktraceResult.OnLimitReached(resultReport);
            }
            throw new NotImplementedException();
            //generate minidump and add minidump to report 
            //string minidumpPath = Database.GenerateMiniDump(report, MiniDumpType);
            //if (!string.IsNullOrEmpty(minidumpPath))
            //{
            //    report.SetMinidumpPath(minidumpPath);
            //}
            //create a JSON payload instance
            var data = report.ToBacktraceData(Attributes);

            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = _backtraceApi.Send(data);

            //clear minidumps generated by app
            //Database.ClearMiniDump(report.MinidumpFile);
            return result;
        }
#if !NET35

        /// <summary>
        /// Send asynchronous report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual async Task<BacktraceResult> SendAsync(BacktraceReportBase<T> report)
        {
            //check rate limiting
            bool watcherValidation = _reportWatcher.WatchReport(report);
            if (!watcherValidation)
            {
                var resultReport = report as BacktraceReport;
                OnClientReportLimitReached?.Invoke(resultReport);
                return BacktraceResult.OnLimitReached(resultReport);
            }
            throw new NotImplementedException();
            //generate minidump and add minidump to report 
            //string minidumpPath = Database.GenerateMiniDump(report, MiniDumpType);
            //if (!string.IsNullOrEmpty(minidumpPath))
            //{
            //    report.SetMinidumpPath(minidumpPath);
            //}
            //create a JSON payload instance
            var data = new BacktraceData<T>(report, Attributes);
            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = await _backtraceApi.SendAsync(data);
            //clear minidumps generated by app
            //Database.ClearMiniDump(report.MinidumpFile);
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
            SendAsync(new BacktraceReportBase<T>(e.Exception)).Wait();
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
            var exception = e.ExceptionObject as Exception;
            var result = SendAsync(new BacktraceReportBase<T>(exception)).Result;
            OnUnhandledApplicationException?.Invoke(exception);
        }
#endif
    }
}

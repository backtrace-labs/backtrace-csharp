using Backtrace.Model;
using Backtrace.Interfaces;
using System;
using System.Collections.Generic;
using Backtrace.Services;
using Backtrace.Types;

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
        public Action<string, string, byte[]> RequestHandler
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
        public Action<Exception> WhenServerUnvailable
        {
            get
            {
                return _backtraceApi.WhenServerUnvailable;
            }
            set
            {
                _backtraceApi.WhenServerUnvailable = value;
            }
        }

        /// <summary>
        /// Set an event executed when Backtrace API return information about send report
        /// </summary>
        public Action<BacktraceServerResponse> OnServerAnswer
        {
            get
            {
                return _backtraceApi.OnServerAnswer;
            }
            set
            {
                _backtraceApi.OnServerAnswer = value;
            }
        }

        /// <summary>
        /// Get or set minidump type
        /// </summary>
        public MiniDumpType MiniDumpType { get; set; } = MiniDumpType.Normal;

        /// <summary>
        /// Set event executed when client site report limit reached
        /// </summary>
        public Action OnClientSiteRatingLimit = null;

        /// <summary>
        /// Set event executed before sending data to Backtrace API
        /// </summary>
        public Func<BacktraceData<T>, BacktraceData<T>> BeforeSend = null;

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
        private readonly BacktraceDatabase<T> _database;

        /// <summary>
        /// Backtrace report watcher that controls number of request sending per minute
        /// </summary>
        private readonly ReportWatcher<T> _reportWatcher;


        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase<T>(databaseDirectory);
            _backtraceApi = new BacktraceApi<T>(backtraceCredentials);
            _reportWatcher = new ReportWatcher<T>(reportPerMin);
        }

        /// <summary>
        /// Send a report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual void Send(BacktraceReportBase<T> report)
        {
            //check rate limiting
            bool watcherValidation = _reportWatcher.WatchReport(report);
            if (!watcherValidation)
            {
                OnClientSiteRatingLimit?.Invoke();
                return;
            }
            //generate minidump and add minidump to report 
            string minidumpPath = _database.GenerateMiniDump(report, MiniDumpType);
            if (!string.IsNullOrEmpty(minidumpPath))
            {
                report._attachmentPaths.Add(minidumpPath);
            }
            //create a JSON payload instance
            var data = new BacktraceData<T>(report, Attributes);

            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            _backtraceApi.Send(data);

            //clear minidumps generated by app
            _database.ClearMiniDump(minidumpPath);
        }
#if !NET35
        /// <summary>
        /// Add automatic exception handling for current application
        /// </summary>
        public virtual void HandleApplicationException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var assembly = System.Reflection.Assembly.GetCallingAssembly();
            var exception = e.ExceptionObject as Exception;
            var report = new BacktraceReportBase<T>(exception, assembly);
            Send(report);
        }
#endif
    }
}

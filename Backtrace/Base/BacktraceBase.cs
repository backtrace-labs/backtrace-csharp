using Backtrace.Model;
using Backtrace.Interfaces;
using System;
using System.Collections.Generic;
using Backtrace.Services;

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
        /// Set an event executed when server return information after sending data to API
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
        /// Set event executed when client site report limit reached
        /// </summary>
        public Action OnClientSiteRatingLimit = null;

        /// <summary>
        /// Set event executed before data send to Backtrace API
        /// </summary>
        public Func<BacktraceData<T>, BacktraceData<T>> BeforeSend = null;

        /// <summary>
        /// Get scoped attributes from Backtrace client. Every argument stored in dictionary will be send to a Backtrace service
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
        /// Instance of request object to Backtrace API
        /// </summary>
        internal IBacktraceApi<T> _backtraceApi;

        /// <summary>
        /// Backtrace database
        /// </summary>
        private readonly BacktraceDatabase<T> _database;

        /// <summary>
        /// Control available request send per minute
        /// </summary>
        private readonly ReportWatcher<T> _reportWatcher;

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Numbers of report send per one sec. If value is equal to zero, there is no request send to API. Value have to be greater than or equal to 0</param>
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
            //generate minidump and add minidump to report if exists
            string minidumpPath = _database.GenerateMiniDump(report);
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
    }
}

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
    public class Backtrace<T>
    {

        /// <summary>
        /// Set an event executed before data send to Backtrace API
        /// </summary>
        public Action<BacktraceData<T>> BeforeSend = null;

        /// <summary>
        /// Get or set request timeout
        /// </summary>
        public int Timeout
        {
            get
            {
                return _backtraceApi.Timeout;
            }
            set
            {
                _backtraceApi.Timeout = value;
            }
        }

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
        private readonly BacktraceDatabase _database;

        /// <summary>
        /// Background watcher
        /// </summary>
        private readonly ReportWatcher<T> _backgroundWatcher;

        private Func<BacktraceReport<T>, bool> _validate;

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Numbers of report send per one sec. If value is equal to zero, there is no request send to API. Value have to be greater than or equal to 0</param>
        public Backtrace(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            string databaseDirectory = "",
            int reportPerMin = 3)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase(databaseDirectory); _backtraceApi = new BacktraceApi<T>(backtraceCredentials);
            //if (reportPerMin != 0)
            //{
            //    _backgroundWatcher = new ReportWatcher<T>(2);
            //    _validate = _backgroundWatcher.WatchReport;
            //}


        }

        /// <summary>
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual void Send(BacktraceReport<T> report)
        {
            _validate?.Invoke(report);
            //create a JSON payload instance
            var data = new BacktraceData<T>(report, Attributes);
            BeforeSend?.Invoke(data);
            bool validRequest = _backtraceApi.Send(data);
        }
    }
}

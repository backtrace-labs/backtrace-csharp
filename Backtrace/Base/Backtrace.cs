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
        public Backtrace(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase<T>(databaseDirectory); _backtraceApi = new BacktraceApi<T>(backtraceCredentials);
            _reportWatcher = new ReportWatcher<T>(reportPerMin);
        }

        /// <summary>
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual bool Send(BacktraceReport<T> report)
        {
            bool watcherValidation = _reportWatcher.WatchReport(report);
            _database.GenerateMiniDump(report);
            //create a JSON payload instance
            var data = new BacktraceData<T>(report, Attributes);
            if (!watcherValidation)
            {
                _database.SaveReport(data);
                return false;
            }
            BeforeSend?.Invoke(data);
            return _backtraceApi.Send(data);
        }
    }
}

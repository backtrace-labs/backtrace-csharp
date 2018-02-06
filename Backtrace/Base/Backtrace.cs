using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private readonly BacktraceApi _backtraceApi;

        /// <summary>
        /// Backtrace database
        /// </summary>
        private readonly BacktraceDatabase _database;

        /// <summary>
        /// Background watcher
        /// </summary>
        private readonly BackgroundWatcher _backgroundWatcher;

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec.</param>
        public Backtrace(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, T> attributes = null,
            string databaseDirectory = "",
            int reportPerSec = 3)
        {
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase(databaseDirectory);
            _backgroundWatcher = new BackgroundWatcher(reportPerSec);
            _backtraceApi = new BacktraceApi(backtraceCredentials);
        }

        /// <summary>
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual void Send(BacktraceReport<T> report)
        {
            //create a JSON payload instance
            var data = new BacktraceData<T>(report, Attributes);
            BeforeSend?.Invoke(data);
            //Send it to API
            throw new NotImplementedException();
        }



    }
}

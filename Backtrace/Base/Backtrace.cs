using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.Base
{
    /// <summary>
    /// Backtrace .NET Client 
    /// </summary>
    public class Backtrace<T>
    {
        /// <summary>
        /// Client attributes
        /// </summary>
        protected Dictionary<string, T> _attributes;

        /// <summary>
        /// Backtrace Credentials information
        /// </summary>
        private readonly BacktraceCredentials _backtraceCredentials;

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
            _backtraceCredentials = backtraceCredentials;
            _attributes = attributes ?? new Dictionary<string, T>();
            _database = new BacktraceDatabase(databaseDirectory);
            _backgroundWatcher = new BackgroundWatcher(reportPerSec);
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
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual void Send(BacktraceReport<T> report)
        {
            //prepare JSON
            //Send it to API
            throw new NotImplementedException();
        }

        

    }
}

using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model.Database
{
    /// <summary>
    /// Backtrace library database settings
    /// </summary>
    public class BacktraceDatabaseSettings
    {
        public BacktraceDatabaseSettings(string path)
        {
            DatabasePath = path;
        }
        /// <summary>
        /// Directory path where reports and minidumps are stored
        /// </summary>
        public string DatabasePath { get; private set; }

        /// <summary>
        /// Maximum number of stored reports in Database. If value is equal to zero, then limit not exists
        /// </summary>
        public uint MaxEntryNumber { get; set; } = 0;

        /// <summary>
        /// Maximum database size in MB. If value is equal to zero, then size is unlimited
        /// </summary>
        public long MaxDatabaseSize { get; set; } = 0;

        /// <summary>
        /// Resend report when http client throw exception
        /// </summary>
        public bool AutoSendMode { get; set; } = false;

        /// <summary>
        /// Retry behaviour
        /// </summary>
        public RetryBehavior RetryBehavior { get; set; } = RetryBehavior.ByInterval;

        /// <summary>
        /// How much seconds library should wait before next retry.
        /// </summary>
        public uint RetryInterval { get; set; } = 5;

        /// <summary>
        /// Maximum number of retries
        /// </summary>
        public uint MaxRetries { get; set; } = 3;

        public RetryOrder RetryOrder { get; set; } = RetryOrder.Stack;
    }
}

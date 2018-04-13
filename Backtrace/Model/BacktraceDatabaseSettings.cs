using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Backtrace library database settings
    /// </summary>
    public class BacktraceDatabaseSettings
    {
        /// <summary>
        /// Directory path where reports and minidumps are stored
        /// </summary>
        public string DatabasePath { get; set; }

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
        /// How much secounds library should wait before next retry.
        /// </summary>
        public uint RetryTime { get; set; } = 30;

        /// <summary>
        /// Total number of retries
        /// </summary>
        public uint TotalRetry { get; set; } = 3;

        public RetryOrder RetryOrder { get; set; } = RetryOrder.Stack;
    }
}

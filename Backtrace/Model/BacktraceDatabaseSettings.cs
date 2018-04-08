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
        /// Initialize new instance of BacktraceDatabaseSettings
        /// </summary>
        public BacktraceDatabaseSettings()
        {
        }

        /// <summary>
        /// Initialize new instance of BacktraceDatabaseSettings. To support previous version of library, internal constructor accepts database path
        /// </summary>
        /// <param name="databasePath"></param>
        internal BacktraceDatabaseSettings(string databasePath)
        {
            DatabasePath = databasePath;
        }
        /// <summary>
        /// Directory path where reports and minidumps are stored
        /// </summary>
        public string DatabasePath { get; set; }

        /// <summary>
        /// Maximum number of stored reports in Database. If value is equal to zero, then limit not exists
        /// </summary>
        public uint MaxReportNumber { get; set; } = 0;

        /// <summary>
        /// Maximum database size in KB. If value is equal to zero, then size is unlimited
        /// </summary>
        public long MaxDatabaseSize { get; set; } = 0;

        /// <summary>
        /// How much secounds library should wait before next retry.
        /// </summary>
        public uint RetryTime { get; set; } = 30;

        /// <summary>
        /// Total number of retries
        /// </summary>
        public uint TotalRetry { get; set; } = 3;
    }
}

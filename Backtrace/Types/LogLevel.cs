using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Types
{
    /// <summary>
    /// Use log level information to send more detailed logs to Backtrace API
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// Trace information
        /// </summary>
        Trace = 1,
        /// <summary>
        /// Debug information
        /// </summary>
        Debug = 2,
        /// <summary>
        /// Default information
        /// </summary>
        Information = 4,
        /// <summary>
        /// Warning information
        /// </summary>
        Warning = 8,
        /// <summary>
        /// Error information
        /// </summary>
        Error = 16,
        /// <summary>
        /// Critical information
        /// </summary>
        Critical = 32

    }
}

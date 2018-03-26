using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Extensions
{
    /// <summary>
    /// Extensions method available for every excepton object
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Convert current exception to BacktraceReport instance
        /// </summary>
        /// <param name="source">Current exception</param>
        /// <returns>Backtrace Report</returns>
        public static BacktraceReport ToBacktraceReport(this Exception source)
        {
            return new BacktraceReport(source);
        }
    }
}

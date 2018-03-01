using Backtrace.Model;
using System;

namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace client interface. Use this interface with dependency injection features
    /// </summary>
    public interface IBacktraceClient<T>
    {
        /// <summary>
        /// Send a new report to a Backtrace service
        /// </summary>
        /// <param name="report">New backtrace report</param>
        bool Send(BacktraceReport report);
    }
}
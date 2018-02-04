using System;

namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace client interface. Use this interface with dependency injection features
    /// </summary>
    public interface IBacktraceClient
    {
        /// <summary>
        /// Send a new report to a Backtrace service
        /// </summary>
        /// <param name="report">New backtrace report</param>
        void Send(BacktraceReport report);

        /// <summary>
        /// Asynchronous way to send a new report to a Backtrace service
        /// </summary>
        /// <param name="report"></param>
        /// <returns>Sending task object</returns>
        //Task SendAsync(BacktraceReport report);
    }
}
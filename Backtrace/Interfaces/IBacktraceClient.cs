using Backtrace.Model;
using System;
#if !NET35
using System.Threading.Tasks;
#endif

namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace client interface. Use this interface with dependency injection features
    /// </summary>
    public interface IBacktraceClient
    {
        /// <summary>
        /// Send a new report to a Backtrace API
        /// </summary>
        /// <param name="report">New backtrace report</param>
        BacktraceResult Send(BacktraceReport report);

#if !NET35
        /// <summary>
        /// Send new asynchronous report to a Backtrace API
        /// </summary>
        /// <param name="report">New backtrace report</param>
        Task<BacktraceResult> SendAsync(BacktraceReport report);
#endif
    }
}
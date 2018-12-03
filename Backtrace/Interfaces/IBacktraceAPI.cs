using Backtrace.Model;
using System;

namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace API sender interface
    /// </summary>
    public interface IBacktraceApi : IDisposable
    {
        /// <summary>
        /// Send a Backtrace report to Backtrace API
        /// </summary>
        /// <param name="data">Library diagnostic data</param>
        BacktraceResult Send(BacktraceData data);

#if !NET35
        /// <summary>
        /// Send asynchronous Backtrace report to Backtrace API
        /// </summary>
        /// <param name="data">Library diagnostic data</param>
        System.Threading.Tasks.Task<BacktraceResult> SendAsync(BacktraceData data);
#endif
        /// <summary>
        /// Set an event executed when received bad request, unauthorize request or other information from server
        /// </summary>
        Action<Exception> OnServerError { get; set; }

        /// <summary>
        /// Set an event executed when server return information after sending data to API
        /// </summary>
        Action<BacktraceResult> OnServerResponse { get; set; }

        /// <summary>
        /// Set custom request method to prepare HTTP request to Backtrace API
        /// </summary>
        Func<string, string, BacktraceData, BacktraceResult> RequestHandler { get; set; }

        void SetClientRateLimitEvent(Action<BacktraceReport> onClientReportLimitReached);

        void SetClientRateLimit(uint rateLimit);
    }
}

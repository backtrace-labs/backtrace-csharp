using Backtrace.Model;
using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace API sender interface
    /// </summary>
    /// <typeparam name="T">Attribute type</typeparam>
    public interface IBacktraceApi<T> : IDisposable
    {
        /// <summary>
        /// Send a Backtrace report to Backtrace API
        /// </summary>
        /// <param name="data">Library diagnostic data</param>
        BacktraceResult Send(BacktraceData<T> data);

#if !NET35
        /// <summary>
        /// Send asynchronous Backtrace report to Backtrace API
        /// </summary>
        /// <param name="data">Library diagnostic data</param>
        System.Threading.Tasks.Task<BacktraceResult> SendAsync(BacktraceData<T> data);
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
        Func<string, string, BacktraceData<T>, BacktraceResult> RequestHandler { get; set; }

        void SetClientRateLimitEvent(Action<BacktraceReport> onClientReportLimitReached);
    }
}

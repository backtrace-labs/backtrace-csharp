using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace API sender interface
    /// </summary>
    /// <typeparam name="T">Attribute type</typeparam>
    public interface IBacktraceApi<T>
    {
        /// <summary>
        /// Send a Backtrace report to Backtrace API
        /// </summary>
        /// <param name="data">Library diagnostic data</param>
        void Send(BacktraceData<T> data);

        /// <summary>
        /// Set an event executed when received bad request, unauthorize request or other information from server
        /// </summary>
        Action<Exception> WhenServerUnvailable { get; set; }

        /// <summary>
        /// Set an event executed when server return information after sending data to API
        /// </summary>
        Action<BacktraceServerResponse> OnServerAnswer { get; set; }

        /// <summary>
        /// Use asynchronous method to send report to server
        /// </summary>
        bool AsynchronousRequest { get; set; }
    }
}

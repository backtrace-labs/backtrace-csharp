using System;
using Backtrace.Model;
using System.Collections.Generic;
using System.Text;
using Backtrace.Interfaces;
using Newtonsoft.Json;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace
{
    /// <summary>
    /// Create requests to Backtrace API
    /// </summary>
    internal class BacktraceApi<T> : IBacktraceApi<T>
    {
        /// <summary>
        /// Get or set request timeout value in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        private readonly BacktraceCredentials _credentials;

        /// <summary>
        /// Create a new instance of Backtrace API request.
        /// </summary>
        /// <param name="credentials">API credentials</param>
        /// <param name="timeout">Request timeout in milliseconds</param>
        public BacktraceApi(BacktraceCredentials credentials, int timeout = 5000)
        {
            _credentials = credentials;
        }

        public void Send(BacktraceData<T> data)
        {
            var json = JsonConvert.SerializeObject(data);
            throw new NotImplementedException();
        }
    }
}

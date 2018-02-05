using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace
{
    /// <summary>
    /// Create requests to Backtrace API
    /// </summary>
    internal class BacktraceApi
    {
        /// <summary>
        /// Get or set request timeout value in milliseconds
        /// </summary>
        internal int Timeout { get; set; }
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

        internal void Send()
        {
            throw new NotImplementedException();
        }
    }
}

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
        private readonly BacktraceCredentials _credentials;
        public BacktraceApi(BacktraceCredentials credentials)
        {
            _credentials = credentials;
        }

        internal void Send()
        {
            throw new NotImplementedException();
        }
    }
}

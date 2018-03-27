using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Types
{
    /// <summary>
    /// Existing server response types
    /// </summary>
    public enum BacktraceResultType
    {
        /// <summary>
        /// Set when client limit is reached
        /// </summary>
        LimitReached,
        /// <summary>
        /// Set when error occurs while sending diagnostic data
        /// </summary>
        ServerError,
        /// <summary>
        /// Set when data were send to API
        /// </summary>
        Ok
    }
}

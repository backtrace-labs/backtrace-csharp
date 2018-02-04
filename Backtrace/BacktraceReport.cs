using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace
{
    /// <summary>
    /// Capture a report of an application
    /// </summary>
    public class BacktraceReport
    {
        /// <summary>
        /// Additional information about report. You can define any information that will be sended to server
        /// </summary>
        private Dictionary<string, string> _attributes;

        /// <summary>
        /// Get an report attributes
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Sending a report with custom message
        /// </summary>
        /// <param name="message">message about application state</param>
        /// <param name="attributes">Report additional information</param>
        public BacktraceReport(
            string message,
            Dictionary<string, string> attributes = null)
        {
            _attributes = attributes ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Sending a report with custom exception
        /// </summary>
        /// <param name="exception">Occur exception</param>
        /// <param name="attributes">Report additional information</param>
        public BacktraceReport(
            Exception exception,
            Dictionary<string, string> attributes = null)
        {
            _attributes = attributes ?? new Dictionary<string, string>();
        }
        /// <summary>
        /// Prepare a report data to send to a Backtrace API
        /// </summary>
        /// <param name="clientAttributes">Client scoped attributes</param>
        internal void PrepareRecord(Dictionary<string,string> clientAttributes)
        {
            throw new NotImplementedException();
        }
    }
}

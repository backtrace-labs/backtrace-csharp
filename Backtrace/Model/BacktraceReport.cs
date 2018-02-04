using Backtrace.Comparers;
using Backtrace.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Capture a report of an application
    /// </summary>
    public class BacktraceReport<T>
    {
        /// <summary>
        /// Additional information about report. You can define any information that will be sended to server
        /// </summary>
        private Dictionary<string, T> _attributes;

        /// <summary>
        /// Get an report attributes
        /// </summary>
        public Dictionary<string, T> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        private readonly string _message;

        /// <summary>
        /// Get a message from report
        /// </summary>
        public string Message => _message;

        private readonly Exception _exception;

        /// <summary>
        /// Get an exception from report
        /// </summary>
        public Exception Exception => _exception;

        /// <summary>
        /// Sending a report with custom message
        /// </summary>
        /// <param name="message">message about application state</param>
        /// <param name="attributes">Report additional information</param>
        public BacktraceReport(
            string message,
            Dictionary<string, T> attributes = null)
        {
            _message = message;
            _attributes = attributes ?? new Dictionary<string, T>();
        }

        /// <summary>
        /// Sending a report with custom exception
        /// </summary>
        /// <param name="exception">Occur exception</param>
        /// <param name="attributes">Report additional information</param>
        public BacktraceReport(
            Exception exception,
            Dictionary<string, T> attributes = null)
        {
            _exception = exception;
            _attributes = attributes ?? new Dictionary<string, T>();
        }

        internal static Dictionary<string, T> ConcatAttributes(
            BacktraceReport<T> report, Dictionary<string, T> attributes)
        {
            if (attributes == null)
            {
                throw new ArgumentException(nameof(attributes));
            }
            var reportAttributes = report._attributes;
            return reportAttributes.Merge(attributes);


        }
    }
}

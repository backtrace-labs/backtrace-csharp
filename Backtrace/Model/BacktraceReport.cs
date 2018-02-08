using Backtrace.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Capture a report of an application
    /// </summary>
    public class BacktraceReport<T>
    {
        /// <summary>
        /// Get an information aboout report type. If value is true the BacktraceReport has an error information
        /// </summary>
        public bool ExceptionTypeReport = false;

        /// <summary>
        /// Additional information about report. You can define any information that will be sended to server
        /// </summary>
        private Dictionary<string, T> _attributes;

        /// <summary>
        /// Get a report classification 
        /// </summary>
        public string Classifier
        {
            get
            {
                if (ExceptionTypeReport)
                {
                    return _exception.GetType().FullName;
                }
                return string.Empty;
            }
        }

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
        /// Get an assembly where client called
        /// </summary>
        internal Assembly CallingAssembly;

        /// <summary>
        /// Sending a report with custom message
        /// </summary>
        /// <param name="message">message about application state</param>
        /// <param name="attributes">Report additional information</param>
        public BacktraceReport(
            string message,
            Dictionary<string, T> attributes = null)
        {
            CallingAssembly = Assembly.GetCallingAssembly();
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
            CallingAssembly = Assembly.GetCallingAssembly();
            _attributes = attributes ?? new Dictionary<string, T>();
            //handle null value in exception parameter
            if (exception == null)
            {
                return;
            }
            _exception = exception;
            var type = _exception?.GetType();
            ExceptionTypeReport = true;
        }

        internal static Dictionary<string, T> ConcatAttributes(
            BacktraceReport<T> report, Dictionary<string, T> attributes)
        {
            if (attributes == null)
            {
                return report._attributes;
            }
            var reportAttributes = report._attributes;
            return reportAttributes.Merge(attributes);
        }

        /// <summary>
        /// Convert exception to ExceptionStack
        /// </summary>
        /// <returns>Exception stack based on exception</returns>
        internal ExceptionStack GetExceptionStack()
        {
            if (ExceptionTypeReport)
            {
                return new ExceptionStack(Exception);
            }
            return null;

        }
    }
}

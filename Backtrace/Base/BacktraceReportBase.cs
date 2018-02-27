using Backtrace.Extensions;
using Backtrace.Model.JsonData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Base
{
    /// <summary>
    /// Capture a report of an application
    /// </summary>
    public class BacktraceReportBase<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>s
        public readonly Guid Uuid = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        public readonly long Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// 
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
                    return Exception.GetType().FullName;
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

        /// <summary>
        /// Get a message from report
        /// </summary>
        internal readonly string Message;

        /// <summary>
        /// Get an exception from report
        /// </summary>
        internal Exception Exception;

        /// <summary>
        /// Get an assembly where client called
        /// </summary>
        internal Assembly CallingAssembly;

        /// <summary>
        /// Get all paths to attachments
        /// </summary>
        internal List<string> _attachmentPaths;

        /// <summary>
        /// Sending a report with custom message
        /// </summary>
        /// <param name="message">message about application state</param>
        /// <param name="attributes">Report additional information</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            string message,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        {
            CallingAssembly = Assembly.GetCallingAssembly();
            Message = message;
            _attributes = attributes ?? new Dictionary<string, T>();
            _attachmentPaths = attachmentPaths ?? new List<string>();
        }

        /// <summary>
        /// Sending a report with custom exception
        /// </summary>
        /// <param name="exception">Occur exception</param>
        /// <param name="attributes">Report additional information</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            Exception exception,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        {
            CallingAssembly = Assembly.GetCallingAssembly();
            _attributes = attributes ?? new Dictionary<string, T>();
            _attachmentPaths = attachmentPaths ?? new List<string>();
            //handle null value in exception parameter
            if (exception == null)
            {
                return;
            }
            Exception = exception;
            var type = Exception?.GetType();
            ExceptionTypeReport = true;
        }

        /// <summary>
        /// Convert exception to ExceptionStack
        /// </summary>
        /// <returns>Exception stack based on exception</returns>
        internal IEnumerable<ExceptionStack> GetExceptionStack()
        {
            if (!ExceptionTypeReport)
            {
                return null;
            }
            return ExceptionStack.Convert(Exception);
        }

        internal static Dictionary<string, T> ConcatAttributes(
            BacktraceReportBase<T> report, Dictionary<string, T> attributes)
        {

            var reportAttributes = report.Attributes;
            if (attributes == null)
            {
                return reportAttributes;
            };
            return reportAttributes.Merge(attributes);
        }
    }
}
